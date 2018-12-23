using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OpenPGPExplorer
{
    public class OnePassSignaturePacket : PGPPacket
    {
        public byte Version { private set; get; }        
        public byte SignatureType { private set; get; }
        public byte HashAlgorithm { private set; get; }
        public byte PKAlgorithm { private set; get; }
        public byte Flag { private set; get; }

        //public HashAlgorithm HashAlgo { private set; get; }

        public override void Parse(TreeBuilder tree)
        {
            Version = tree.ReadByte("Version");
            SignatureType = tree.ReadByte("Signature Type", SignatureTypes.Get);
            HashAlgorithm = tree.ReadByte("Hash Algorithm", HashAlgorithmTypes.Get);

            //try
            //{
            //    HashAlgo = null;
            //    HashAlgo = HashAlgorithmTypes.GetHashAlgoManaged(HashAlgorithm);
            //}
            //catch { }

            PKAlgorithm = tree.ReadByte("Primary Key Algorithm", PKAlgorithmTypes.Get);

            tree.ReadBytes("Key ID", 8);
            Flag = tree.ReadByte("Flag");

        }
    }
}
