using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPGPExplorer
{
    public static class CompressionAlgorithmTypes
    {
        private static readonly Dictionary<byte, string> CompressionAlgorithmList = new Dictionary<byte, string>();
        static CompressionAlgorithmTypes()
        {
            CompressionAlgorithmList.Add(0, "Uncompressed");
            CompressionAlgorithmList.Add(1, "ZIP [RFC1951]");
            CompressionAlgorithmList.Add(2, "ZLIB [RFC1950]");
            CompressionAlgorithmList.Add(3, "BZip2 [BZ2]");
        }

        public static string Get(byte Code)
        {
            if (CompressionAlgorithmList.TryGetValue(Code, out string value))
                return value;
            return "Unknown Algorithm";
        }      

    }
}
