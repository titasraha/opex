using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPGPExplorer
{
    public static class PKAlgorithmTypes
    {
        private static readonly Dictionary<byte, string> PKAlgorithmList = new Dictionary<byte, string>();
        static PKAlgorithmTypes()
        {
            PKAlgorithmList.Add(RSA, "RSA (Encrypt or Sign) [HAC]");
            PKAlgorithmList.Add(RSA_ENCRYPT, "RSA Encrypt-Only [HAC]");
            PKAlgorithmList.Add(RSA_SIGN, "RSA Sign-Only [HAC]");
            PKAlgorithmList.Add(Elgamal, "Elgamal (Encrypt-Only) [ELGAMAL] [HAC]");
            PKAlgorithmList.Add(DSA, "DSA (Digital Signature Algorithm) [FIPS186] [HAC]");
            PKAlgorithmList.Add(18, "Reserved for Elliptic Curve");
            PKAlgorithmList.Add(19, "Reserved for ECDSA");
            PKAlgorithmList.Add(20, "Reserved (formerly Elgamal Encrypt or Sign)");
            PKAlgorithmList.Add(21, "Reserved for Diffie-Hellman");
            PKAlgorithmList.Add(100, "Private/Experimental algorithm");
            PKAlgorithmList.Add(101, "Private/Experimental algorithm");
            PKAlgorithmList.Add(102, "Private/Experimental algorithm");
            PKAlgorithmList.Add(103, "Private/Experimental algorithm");
            PKAlgorithmList.Add(104, "Private/Experimental algorithm");
            PKAlgorithmList.Add(105, "Private/Experimental algorithm");
            PKAlgorithmList.Add(106, "Private/Experimental algorithm");
            PKAlgorithmList.Add(107, "Private/Experimental algorithm");
            PKAlgorithmList.Add(108, "Private/Experimental algorithm");
            PKAlgorithmList.Add(109, "Private/Experimental algorithm");
            PKAlgorithmList.Add(110, "Private/Experimental algorithm");
           
        }

        public static string Get(byte Code)
        {
            if (PKAlgorithmList.TryGetValue(Code, out string value))
                return value;
            return "Unknown Algorithm";
        }

        public static readonly byte RSA = 1;
        public static readonly byte RSA_ENCRYPT = 2;
        public static readonly byte RSA_SIGN = 3;
        public static readonly byte Elgamal = 16;
        public static readonly byte DSA = 17;

    }
}
