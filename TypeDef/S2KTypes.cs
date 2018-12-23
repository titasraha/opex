using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPGPExplorer
{
    public static class S2KTypes
    {
        private static readonly Dictionary<byte, string> S2KTypesList = new Dictionary<byte, string>();
        static S2KTypes()
        {
            S2KTypesList.Add(Simple, "Simple S2K");
            S2KTypesList.Add(Salted, "Salted S2K");
            S2KTypesList.Add(2, "Reserved");
            S2KTypesList.Add(IteratedAndSalted, "Iterated and Salted S2K");
            S2KTypesList.Add(GNUExtension, "GNU Extension S2K");


        }

        public static string Get(byte Code)
        {
            if (S2KTypesList.TryGetValue(Code, out string value))
                return value;
            return "Unknown S2K Type";
        }

        public static readonly byte Simple = 0;
        public static readonly byte Salted = 1;
        public static readonly byte IteratedAndSalted = 3;
        public static readonly byte GNUExtension = 101;

    }
}
