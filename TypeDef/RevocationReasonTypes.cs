using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPGPExplorer
{
    public static class RevocationReasonTypes
    {
        private static readonly Dictionary<byte, string> RevocationReasonList = new Dictionary<byte, string>();
        static RevocationReasonTypes()
        {
            RevocationReasonList.Add(0, "No reason specified");
            RevocationReasonList.Add(1, "Key is superseded");
            RevocationReasonList.Add(2, "Key material has been compromised");
            RevocationReasonList.Add(3, "Key is retired and no longer used");
            RevocationReasonList.Add(32, "User ID information is no longer valid");
        }

        public static string Get(byte Code)
        {
            if (RevocationReasonList.TryGetValue(Code, out string value))
                return value;
            return "Unknown Reason";
        }
    }
}
