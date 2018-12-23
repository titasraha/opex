using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
namespace OpenPGPExplorer
{
    public static class OpenPGP 
    {
        
        public static event StatusUpdates StatusUpdate;

        private static List<PacketBlock> _PacketNodes;


        static OpenPGP()
        {
            Reset();
        }

        public static void Reset()
        {
            _PacketNodes = new List<PacketBlock>();
        }

        public static List<PublicKeyPacket> FindPrimaryKey(byte[] Issuer, bool FindSubKeys = false)
        {
            return _PacketNodes
                .Where(n => (n.PGPPacket.PacketTag == 6 || n.PGPPacket.PacketTag == 5 || (FindSubKeys && (n.PGPPacket.PacketTag == 7 || n.PGPPacket.PacketTag == 14))))
                .Select(s => (PublicKeyPacket)s.PGPPacket)
                .Where(p => p.PacketDataPublicKey != null && (p.KeyId.SequenceEqual(Issuer) || (Issuer.Length == 1 && Issuer[0] == 0)))
                .ToList();
        }

        public static PGPPacket GetPreviousPacket(PGPPacket CurrentPacket)
        {
            int Idx = _PacketNodes.FindIndex(w => w.PGPPacket.Equals(CurrentPacket) && w.PGPPacket.FileName == CurrentPacket.FileName);
            if (Idx > 0)
                return _PacketNodes[Idx - 1].PGPPacket;
            return null;
        }

        public static void Validate()
        {

            foreach (var PacketBlock in _PacketNodes)
            {
                PGPPacket pgp = PacketBlock.PGPPacket;

                if (pgp is SignaturePacket Sig)
                {
                    bool HashVerified = false;
                    var PKs = FindPrimaryKey(Sig.Issuer, true);

                    PacketBlock.Type = ByteBlockType.CalculatedError;

                    foreach (var PrimaryKey in PKs)
                    {
                        var PublicKey = PrimaryKey.PublicKeyAlgorithm;
                        //var PublicKeySignature = Sig.PublicKeySignature;
                        HashVerified = PublicKey.VerifySignature(Sig);
                        if (HashVerified)
                            break;
                    }

                    if (HashVerified)
                    {
                        PacketBlock.Message = "Hash: " + BitConverter.ToString(Sig.HashedData);
                        PacketBlock.Type = ByteBlockType.CalculatedSuccess;
                    }
                    else
                        PacketBlock.Message = "Signature is Invalid/Unable to verify";
                }
            }
        }

        public static ByteBlock Process(string PGPFile)
        {
            var Root = new ByteBlock();
            PGPPacket pgp;
            PublicKeyPacket PrimaryKeyPacket = null;
            PublicKeyPacket SubKeyPacket = null;
            UserIDPacket UIDPacket = null;
            Stack<OnePassSignaturePacket> OPSigPacketStack = new Stack<OnePassSignaturePacket>();
            HashAlgorithm[] HashAlgorithms = null;
            

            using (var PacketReader = new PGPReader(PGPFile))
            {

                PacketReader.DoIndexUpdate += IndexUpdate;
                while ((pgp = PacketReader.ReadNextPacket()) != null)
                {

                    TreeBuilder Tree = new TreeBuilder(PacketReader);
                    pgp.Parse(Tree);

                    if (pgp is LiteralDataPacket litPgp)
                    {
                        if (HashAlgorithms == null)
                            HashAlgorithms = GetHashAlgorithms(OPSigPacketStack);

                        litPgp.ProgressUpdate += StatusUpdate;
                        litPgp.DoHash(PacketReader, HashAlgorithms);
                        
                    }
                    else
                    {
                        if (pgp is OnePassSignaturePacket OPSig)
                            OPSigPacketStack.Push(OPSig);
                        else if (pgp.PacketTag == 6 || pgp.PacketTag == 5)
                            PrimaryKeyPacket = (PublicKeyPacket)pgp;
                        else if (pgp is UserIDPacket)
                            UIDPacket = (UserIDPacket)pgp;
                        else if (pgp.PacketTag == 14 || pgp.PacketTag == 7)
                            SubKeyPacket = (PublicKeyPacket)pgp;
                        else if (pgp is SignaturePacket Sig)
                        {

                            if ((Sig.SignatureType == 0x18 || Sig.SignatureType == 0x19) && PrimaryKeyPacket != null && SubKeyPacket != null && SubKeyPacket.PacketDataPublicKey != null)
                                Sig.GenerateSubKeyBindingHash(PrimaryKeyPacket, SubKeyPacket);

                            if (Sig.SignatureType >= 0x10 && Sig.SignatureType <= 0x13 && PrimaryKeyPacket != null && UIDPacket != null)
                                Sig.GenerateCertifyHash(PrimaryKeyPacket, UIDPacket);

                            // Signature of a Binary Document
                            if (Sig.SignatureType == 0x00)
                            {
                                // Are we generating hash for One Pass Signature Packet
                                if (OPSigPacketStack.Count() > 0)
                                {
                                    var OnePassSignaturePacket = OPSigPacketStack.Pop();

                                    if (OnePassSignaturePacket.HashAlgorithm == Sig.HashAlgorithm)
                                        Sig.GenerateBinaryHash(HashAlgorithmTypes.GetHashAlgoManaged(Sig.HashAlgorithm, HashAlgorithms));
                                }

                            }

                        }
                    }


                    PacketBlock pb = new PacketBlock(pgp);
                    pb.AddChildBlock(Tree.StartBlock);
                    Root.AddChildBlock(pb);

                    _PacketNodes.Add(pb);

                }
            }

            return Root.ChildBlock;
        }

        

        private static HashAlgorithm[] GetHashAlgorithms(Stack<OnePassSignaturePacket> Stack)
        {
            byte[] HashAlgorithmCodes = Stack.ToArray().Select(x => x.HashAlgorithm).Distinct().ToArray();

            var HashAlgorithms = HashAlgorithmCodes.Select(code =>
            {
                try
                {
                    return HashAlgorithmTypes.GetHashAlgoManaged(code);
                }
                catch { return null; }
            })
            .Where(algo => algo != null)
            .ToArray();

            return HashAlgorithms;
        }

        private static void IndexUpdate(int Ctr)
        {
            string Msg = "Building Index " + Ctr.ToString();
            StatusUpdate?.Invoke(Msg);

        }

        
        
    }
}
