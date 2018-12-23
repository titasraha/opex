using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPGPExplorer
{
    public static class KeyFlagsTypes
    {
        private static readonly Dictionary<byte, string> KeyFlagsList = new Dictionary<byte, string>();
        static KeyFlagsTypes()
        {
            KeyFlagsList.Add(0x1, "Certify");
            KeyFlagsList.Add(0x2, "Sign");
            KeyFlagsList.Add(0x4, "Encrypt Communication");
            KeyFlagsList.Add(0x8, "Encrypt Storage");
            KeyFlagsList.Add(0x10, "Private component may have been split");
            KeyFlagsList.Add(0x20, "Authentication");
            KeyFlagsList.Add(0x80, "Private key multiple possession");
            
        
           
        }

        public static string Get(byte Code)
        {
            string v = "";

            foreach (var kv in KeyFlagsList)
                if ((kv.Key & Code) != 0)
                    v += kv.Value + ", ";

            if (v != "")
                return v.Substring(0, v.Length - 2);
            
            return "Unknown Flag";
        }

    }
}
