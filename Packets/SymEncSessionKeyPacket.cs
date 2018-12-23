using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPGPExplorer
{
    public class SymEncSessionKeyPacket : PGPPacket
    {
        public byte Version { private set; get; }
        public byte[] EncryptedSessionKey { private set; get; }
        public S2K S2K { private set; get; }

        public override void Parse(TreeBuilder tree)
        {
            S2K = new S2K();

            Version = tree.ReadByte("Version");
            S2K.SymAlgo = tree.ReadByte("Symmetric Algorithm", SymmetricAlgorithmTypes.Get);
            byte S2KSpecifier = tree.ReadByte("S2K Specifier", S2KTypes.Get);

            if (S2KSpecifier != S2KTypes.Salted && S2KSpecifier != S2KTypes.Simple && S2KSpecifier != S2KTypes.IteratedAndSalted)
                throw new InvalidDataException("Invalid S2K Specifier");
           
            S2K.HashAlgorithm = tree.ReadByte("Hash Algorithm", HashAlgorithmTypes.Get);

            if (S2KSpecifier == S2KTypes.Salted || S2KSpecifier == S2KTypes.IteratedAndSalted)
            {
                S2K.Salt = tree.ReadBytes("Salt", 8);
                if (S2KSpecifier == S2KTypes.IteratedAndSalted)
                {
                    byte CodedCount = tree.ReadByte("Coded Iteration");
                    S2K.ByteCount = (16 + (CodedCount & 15)) << ((CodedCount >> 4) + 6);
                }
            }

            if (tree.IsMoreData())
                EncryptedSessionKey = tree.ReadBytes("Encrypted Session Key");
            else
                EncryptedSessionKey = null;          
        }
    }
}
