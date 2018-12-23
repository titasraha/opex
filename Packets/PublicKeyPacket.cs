using System;
using System.Security.Cryptography;

namespace OpenPGPExplorer
{   
    public class PublicKeyPacket: PGPPacket
    {
        
        public byte Version { private set; get; }
        public uint CreationTime { private set; get; }
        public PKAlgorithm PublicKeyAlgorithm { private set; get; }
        public byte[] KeyId { private set; get; }                
        public byte[] PacketDataPublicKey { get; private set; }

        public override void Parse(TreeBuilder tree)
        {
            tree.SetBookMark();
            Version = tree.ReadByte("Version");
            tree.ReadFormatted("Creation Time", BlockFormat.UnixTime);
            if (Version == 2 || Version == 3 || Version == 4)
            {
                if (Version != 4)
                    tree.ReadNumber("Days Valid", 2);
            }
            else
                throw new NotImplementedException("Unknown Public Key Packet Version Code: " + Version.ToString());

            byte AlgoCode = tree.ReadByte("Public Key Algorithm", PKAlgorithmTypes.Get);
            PublicKeyAlgorithm = PKAlgorithm.CreatePKAlgorithm(AlgoCode);

            if (PublicKeyAlgorithm == null)
            {
                tree.ReadBytes("Unknown Public Key Algorithm");
                return;
            }
            
            PublicKeyAlgorithm.LoadPublicKey(tree);
            PacketDataPublicKey = tree.ReadBytesFromBookMark();
                      

            if (Version < 4)
            {
                // Only RSA is supported
                var ModN = ((RSA)PublicKeyAlgorithm).ModN;
                //KeyId = BitConverter.ToString(ModN, ModN.Length - 8);
                KeyId = ModN.SubArray(ModN.Length - 8, 8);
            }
            else
            {
                int l = PacketDataPublicKey.Length;
                SHA1 shaM = new SHA1Managed();

                byte[] HashContext = new byte[l + 3];
                HashContext[0] = 0x99;
                HashContext[1] = (byte)((l & 0xFF00) >> 8);
                HashContext[2] = (byte)(l & 0x00FF);
                Array.Copy(PacketDataPublicKey, 0, HashContext, 3, l);
                byte[] result = shaM.ComputeHash(HashContext);
                //KeyId = BitConverter.ToString(result, result.Length - 8);
                KeyId = result.SubArray(result.Length - 8, 8);
            }
            tree.AddCalculated("Key Id", BitConverter.ToString(KeyId), KeyId);
        }
    }
}
