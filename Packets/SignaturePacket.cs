using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace OpenPGPExplorer
{
    public class SignaturePacket : PGPPacket
    {
        public byte Version { private set; get; }
        public byte SignatureType { private set; get; }
        public byte PKAlgo { private set; get; }
        public byte HashAlgorithm { private set; get; }
        public byte[] Issuer { private set; get; }
        //public PKAlgorithm PublicKeySignature { private set; get; }
        public ITransformedData SecretKeyTransformedData { private set; get; }
        public byte[] HashedSubPacketBytes { private set; get; }
        public byte[] UnHashedSubPacketBytes { private set; get; }
        public byte[] HashedData { private set; get; }

        public HashAlgorithmName HashAlgorithmName
        {
            get
            {
                HashAlgorithmName HashName;

                if (HashAlgorithm == HashAlgorithmTypes.MD5)
                    HashName = HashAlgorithmName.MD5;
                else if (HashAlgorithm == HashAlgorithmTypes.SHA1)
                    HashName = HashAlgorithmName.SHA1;
                else if (HashAlgorithm == HashAlgorithmTypes.SHA256)
                    HashName = HashAlgorithmName.SHA256;
                else if (HashAlgorithm == HashAlgorithmTypes.SHA384)
                    HashName = HashAlgorithmName.SHA384;
                else if (HashAlgorithm == HashAlgorithmTypes.SHA512)
                    HashName = HashAlgorithmName.SHA512;
                return HashName;

            }
        }



        public override void Parse(TreeBuilder tree)
        {
            Version = tree.ReadByte("Version");
            SignatureType = tree.ReadByte("Signature Type", SignatureTypes.Get);
            PKAlgo = tree.ReadByte("Public Key Algorithm", PKAlgorithmTypes.Get);


            if (Version == 4)
            {
                uint SubPacketLength;

                HashAlgorithm = tree.ReadByte("Hash Algorithm", HashAlgorithmTypes.Get);

                SubPacketLength = tree.ReadNumber(2);
                tree.SetBookMark();
                HashedSubPacketBytes = tree.ReadBytes("Hashed Sub Packet", (int)SubPacketLength);
                //tree.Seek(-SubPacketLength);
                tree.GoToBookMark();

                var HashedSubPackets = new SignatureSubPackets();
                HashedSubPackets.Parse(tree, (int)SubPacketLength);


                SubPacketLength = tree.ReadNumber(2);
                tree.SetBookMark();
                UnHashedSubPacketBytes = tree.ReadBytes("Unhashed Sub Packet", (int)SubPacketLength);
                //tree.Seek(-SubPacketLength);
                tree.GoToBookMark();

                var UnHashedSubPackets = new SignatureSubPackets();
                UnHashedSubPackets.Parse(tree, (int)SubPacketLength);
                Issuer = UnHashedSubPackets.Issuer;

                tree.ReadBytes("Left 16 Bit Hash", 2);

                var PublicKeyAlgorithm = PKAlgorithm.CreatePKAlgorithm(PKAlgo);




                SecretKeyTransformedData = null;
                if (PublicKeyAlgorithm == null)
                    tree.ReadBytes("Unknowon Signature");
                else
                    SecretKeyTransformedData = PublicKeyAlgorithm.LoadSecretKeyTransformedData(tree);


                //if (PublicKeySignature != null)
                //{
                //    tree.AddNode("Signature");
                //    tree.AddChild(PublicKeySignature.LoadSignatureValue(tree.ReadMPIBytes));
                //}
                //else
                //    tree.ReadBytes("Signature");                    
            }
            else
                throw new NotImplementedException("Not Implemented");

        }

        public void GenerateSubKeyBindingHash(PublicKeyPacket PKP, PublicKeyPacket SubKey)
        {
            SignaturePacket SigP = this;

            int HashContextLength = 3 + PKP.PacketDataPublicKey.Length +
                3 + SubKey.PacketDataPublicKey.Length +
                6 + SigP.HashedSubPacketBytes.Length
                + 6;

            int idx = 0;

            byte[] HashContext = new byte[HashContextLength];

            HashContext[idx++] = 0x99;
            HashContext[idx++] = (byte)((PKP.PacketDataPublicKey.Length & 0xFF00) >> 8);
            HashContext[idx++] = (byte)(PKP.PacketDataPublicKey.Length & 0x00FF);
            Array.Copy(PKP.PacketDataPublicKey, 0, HashContext, idx, PKP.PacketDataPublicKey.Length);
            idx += PKP.PacketDataPublicKey.Length;

            HashContext[idx++] = 0x99;
            HashContext[idx++] = (byte)((SubKey.PacketDataPublicKey.Length & 0xFF00) >> 8);
            HashContext[idx++] = (byte)(SubKey.PacketDataPublicKey.Length & 0x00FF);
            Array.Copy(SubKey.PacketDataPublicKey, 0, HashContext, idx, SubKey.PacketDataPublicKey.Length);
            idx += SubKey.PacketDataPublicKey.Length;

            HashContext[idx++] = SigP.Version;
            HashContext[idx++] = SigP.SignatureType;
            HashContext[idx++] = SigP.PKAlgo;
            HashContext[idx++] = SigP.HashAlgorithm;  // Version 4
            HashContext[idx++] = (byte)((SigP.HashedSubPacketBytes.Length & 0xFF00) >> 8);
            HashContext[idx++] = (byte)(SigP.HashedSubPacketBytes.Length & 0x00FF);
            Array.Copy(SigP.HashedSubPacketBytes, 0, HashContext, idx, SigP.HashedSubPacketBytes.Length);
            var HashedSubPacketLength = SigP.HashedSubPacketBytes.Length;
            idx += HashedSubPacketLength;

            HashContext[idx++] = 0x04;
            HashContext[idx++] = 0xFF;

            HashedSubPacketLength += 6; // Add the bytes before the hashed data

            HashContext[idx++] = (byte)((HashedSubPacketLength & 0xFF000000) >> 24);
            HashContext[idx++] = (byte)((HashedSubPacketLength & 0x00FF0000) >> 16);
            HashContext[idx++] = (byte)((HashedSubPacketLength & 0x0000FF00) >> 8);
            HashContext[idx++] = (byte)(HashedSubPacketLength & 0x000000FF);

            HashedData = ComputeHash(HashContext);

        }

        public void GenerateBinaryHash(HashAlgorithm HashAlgo)
        {
            if (HashAlgo == null)
                return;

            int HashContextLength = 6 + HashedSubPacketBytes.Length + 6;

            byte[] HashContext = new byte[HashContextLength];

                

            int idx = 0;

            HashContext[idx++] = Version;
            HashContext[idx++] = SignatureType;
            HashContext[idx++] = PKAlgo;
            HashContext[idx++] = HashAlgorithm;  // Version 4
            HashContext[idx++] = (byte)((HashedSubPacketBytes.Length & 0xFF00) >> 8);
            HashContext[idx++] = (byte)(HashedSubPacketBytes.Length & 0x00FF);

            Array.Copy(HashedSubPacketBytes, 0, HashContext, idx, HashedSubPacketBytes.Length);
            var HashedSubPacketLength = HashedSubPacketBytes.Length;
            idx += HashedSubPacketLength;

            HashContext[idx++] = 0x04;
            HashContext[idx++] = 0xFF;

            HashedSubPacketLength += 6; // Add the bytes before the hashed data

            HashContext[idx++] = (byte)((HashedSubPacketLength & 0xFF000000) >> 24);
            HashContext[idx++] = (byte)((HashedSubPacketLength & 0x00FF0000) >> 16);
            HashContext[idx++] = (byte)((HashedSubPacketLength & 0x0000FF00) >> 8);
            HashContext[idx++] = (byte)(HashedSubPacketLength & 0x000000FF);

            HashAlgo.TransformFinalBlock(HashContext,0,HashContext.Length);
            HashedData = HashAlgo.Hash;


        }

        public void GenerateCertifyHash(PublicKeyPacket PKP, UserIDPacket UIDP)
        {

            SignaturePacket SigP = this;

            int HashContextLength = 3 + PKP.PacketDataPublicKey.Length +
                5 + UIDP.UserIDBytes.Length +
                6 + SigP.HashedSubPacketBytes.Length
                + 6;

            int idx = 3 + PKP.PacketDataPublicKey.Length;

            byte[] HashContext = new byte[HashContextLength];

            HashContext[0] = 0x99;
            HashContext[1] = (byte)((PKP.PacketDataPublicKey.Length & 0xFF00) >> 8);
            HashContext[2] = (byte)(PKP.PacketDataPublicKey.Length & 0x00FF);
            Array.Copy(PKP.PacketDataPublicKey, 0, HashContext, 3, PKP.PacketDataPublicKey.Length);

            HashContext[idx++] = 0xB4;
            HashContext[idx++] = (byte)((UIDP.UserIDBytes.Length & 0xFF000000) >> 24);
            HashContext[idx++] = (byte)((UIDP.UserIDBytes.Length & 0x00FF0000) >> 16);
            HashContext[idx++] = (byte)((UIDP.UserIDBytes.Length & 0x0000FF00) >> 8);
            HashContext[idx++] = (byte)(UIDP.UserIDBytes.Length & 0x000000FF);
            Array.Copy(UIDP.UserIDBytes, 0, HashContext, idx, UIDP.UserIDBytes.Length);
            idx += UIDP.UserIDBytes.Length;

            HashContext[idx++] = SigP.Version;
            HashContext[idx++] = SigP.SignatureType;
            HashContext[idx++] = SigP.PKAlgo;
            HashContext[idx++] = SigP.HashAlgorithm;  // Version 4
            HashContext[idx++] = (byte)((SigP.HashedSubPacketBytes.Length & 0xFF00) >> 8);
            HashContext[idx++] = (byte)(SigP.HashedSubPacketBytes.Length & 0x00FF);
            Array.Copy(SigP.HashedSubPacketBytes, 0, HashContext, idx, SigP.HashedSubPacketBytes.Length);
            var HashedSubPacketLength = SigP.HashedSubPacketBytes.Length;
            idx += HashedSubPacketLength;

            HashContext[idx++] = 0x04;
            HashContext[idx++] = 0xFF;

            HashedSubPacketLength += 6; // Add the bytes before the hashed data

            HashContext[idx++] = (byte)((HashedSubPacketLength & 0xFF000000) >> 24);
            HashContext[idx++] = (byte)((HashedSubPacketLength & 0x00FF0000) >> 16);
            HashContext[idx++] = (byte)((HashedSubPacketLength & 0x0000FF00) >> 8);
            HashContext[idx++] = (byte)(HashedSubPacketLength & 0x000000FF);

            HashedData = ComputeHash(HashContext);

        }

        private byte[] ComputeHash(byte[] HashContext)
        {
            HashAlgorithmName n = HashAlgorithmName;
            string s = n.Name;

            if (!string.IsNullOrEmpty(s))
            {
                HashAlgorithm HashAlgo = (HashAlgorithm)CryptoConfig.CreateFromName(s);


                byte[] result = HashAlgo.ComputeHash(HashContext);

                return result;
            }
            return null;
        }
    }
}
