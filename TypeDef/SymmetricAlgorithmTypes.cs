using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPGPExplorer
{
    public static class SymmetricAlgorithmTypes
    {
        private static readonly Dictionary<byte, SymmetricAlgorithmInfo> SymmetricAlgorithmList = new Dictionary<byte, SymmetricAlgorithmInfo>();
        static SymmetricAlgorithmTypes()
        {
            SymmetricAlgorithmList.Add(0, new SymmetricAlgorithmInfo { Description = "Unencrypted", BlockSize = 0 });
            SymmetricAlgorithmList.Add(1, new SymmetricAlgorithmInfo { Description = "IDEA", BlockSize = 64, KeySize = 128});
            SymmetricAlgorithmList.Add(2, new SymmetricAlgorithmInfo { Description = "TripleDES DES-EDE, [SCHNEIER] [HAC]", BlockSize = 64, KeySize = 168 });
            SymmetricAlgorithmList.Add(3, new SymmetricAlgorithmInfo { Description = "CAST5 (128 bit key, as per [RFC2144])", BlockSize = 64, KeySize = 128 });
            SymmetricAlgorithmList.Add(4, new SymmetricAlgorithmInfo { Description = "Blowfish (128 bit key, 16 rounds) [BLOWFISH]", BlockSize = 64, KeySize = 128 });
            SymmetricAlgorithmList.Add(5, new SymmetricAlgorithmInfo { Description = "Reserved", BlockSize = 0 });
            SymmetricAlgorithmList.Add(6, new SymmetricAlgorithmInfo { Description = "Reserved", BlockSize = 0 });
            SymmetricAlgorithmList.Add(AES128, new SymmetricAlgorithmInfo { Description = "AES with 128-bit key", BlockSize = 128, KeySize = 128 });
            SymmetricAlgorithmList.Add(8, new SymmetricAlgorithmInfo { Description = "AES with 192-bit key", BlockSize = 128, KeySize = 192 });
            SymmetricAlgorithmList.Add(9, new SymmetricAlgorithmInfo { Description = "AES with 256-bit key", BlockSize = 128, KeySize = 256 });
            SymmetricAlgorithmList.Add(10, new SymmetricAlgorithmInfo { Description = "Twofish with 256-bit key", BlockSize = 128, KeySize = 256 });
        }

        public static string Get(byte Code)
        {
            if (SymmetricAlgorithmList.TryGetValue(Code, out SymmetricAlgorithmInfo value))
                return value.Description;
            return "Unknown Algorithm";
        }

        public static int GetBlockSize(byte Code)
        {
            if (SymmetricAlgorithmList.TryGetValue(Code, out SymmetricAlgorithmInfo value))
                return value.BlockSize;
            return -1;
        }

        public static int GetKeySize(byte Code)
        {
            if (SymmetricAlgorithmList.TryGetValue(Code, out SymmetricAlgorithmInfo value))
                return value.KeySize;
            return 0;
        }

        public static readonly byte AES128 = 7;
        public static readonly byte AES192 = 8;
        public static readonly byte AES256 = 9;


    }

    public class SymmetricAlgorithmInfo
    {
        public string Description;
        public int BlockSize;
        public int KeySize;
    }
}
