using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OpenPGPExplorer
{
    public static class HashAlgorithmTypes
    {
        private static readonly Dictionary<byte, string> HashAlgorithmList = new Dictionary<byte, string>();
        static HashAlgorithmTypes()
        {
            HashAlgorithmList.Add(MD5, "MD5 [HAC]");
            HashAlgorithmList.Add(SHA1, "SHA-1 [FIPS180]");
            HashAlgorithmList.Add(RIPEMD160, "RIPE-MD/160 [HAC]");
            HashAlgorithmList.Add(4, "Reserved");
            HashAlgorithmList.Add(5, "Reserved");
            HashAlgorithmList.Add(6, "Reserved");
            HashAlgorithmList.Add(7, "Reserved");
            HashAlgorithmList.Add(SHA256, "SHA256 [FIPS180]");
            HashAlgorithmList.Add(SHA384, "SHA384 [FIPS180]");
            HashAlgorithmList.Add(SHA512, "SHA512 [FIPS180]");
            HashAlgorithmList.Add(SHA224, "SHA224 [FIPS180]");
            HashAlgorithmList.Add(100, "Private/Experimental algorithm");
            HashAlgorithmList.Add(101, "Private/Experimental algorithm");
            HashAlgorithmList.Add(102, "Private/Experimental algorithm");
            HashAlgorithmList.Add(103, "Private/Experimental algorithm");
            HashAlgorithmList.Add(104, "Private/Experimental algorithm");
            HashAlgorithmList.Add(105, "Private/Experimental algorithm");
            HashAlgorithmList.Add(106, "Private/Experimental algorithm");
            HashAlgorithmList.Add(107, "Private/Experimental algorithm");
            HashAlgorithmList.Add(108, "Private/Experimental algorithm");
            HashAlgorithmList.Add(109, "Private/Experimental algorithm");
            HashAlgorithmList.Add(110, "Private/Experimental algorithm");
        
           
        }

        public static string Get(byte Code)
        {
            if (HashAlgorithmList.TryGetValue(Code, out string value))
                return value;
            return "Unknown Algorithm";
        }

        public static HashAlgorithm GetHashAlgoManaged(byte HashAlgo)
        {
            if (HashAlgo == SHA1)
                return new SHA1Managed();
            else if (HashAlgo == RIPEMD160)
                return new RIPEMD160Managed();
            else if (HashAlgo == SHA256)
                return new SHA256Managed();
            else if (HashAlgo == SHA384)
                return new SHA384Managed();
            else if (HashAlgo == SHA512)
                return new SHA512Managed();
            else
                throw new NotImplementedException(Get(HashAlgo) + " (" + HashAlgo.ToString() + ") not supported");
        }

        public static HashAlgorithm GetHashAlgoManaged(byte HashAlgo, HashAlgorithm[] HashAlgorithms)
        {
            foreach(var HashAlgorithm in HashAlgorithms)
            {
                if (HashAlgo == SHA1 && HashAlgorithm is SHA1Managed)
                    return HashAlgorithm;
                else if (HashAlgo == RIPEMD160 && HashAlgorithm is RIPEMD160Managed)
                    return HashAlgorithm;
                else if (HashAlgo == SHA256 && HashAlgorithm is SHA256Managed)
                    return HashAlgorithm;
                else if (HashAlgo == SHA384 && HashAlgorithm is SHA384Managed)
                    return HashAlgorithm;
                else if (HashAlgo == SHA512 && HashAlgorithm is SHA512Managed)
                    return HashAlgorithm;
            }
            return null;
        }

        public static readonly byte MD5 = 1;
        public static readonly byte SHA1 = 2;
        public static readonly byte RIPEMD160 = 3;
        public static readonly byte SHA256 = 8;
        public static readonly byte SHA384 = 9;
        public static readonly byte SHA512 = 10;
        public static readonly byte SHA224 = 11;

    }
}
