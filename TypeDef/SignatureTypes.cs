using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPGPExplorer
{
    public static class SignatureTypes
    {
        private static readonly Dictionary<byte, string> SignatureTypesList = new Dictionary<byte, string>();
        static SignatureTypes()
        {
            SignatureTypesList.Add(0x0, "Signature of a binary document");
            SignatureTypesList.Add(0x1, "Signature of a canonical text document");
            SignatureTypesList.Add(0x2, "Standalone signature");
            SignatureTypesList.Add(0x10, "Generic certification of a User ID and Public-Key packet");
            SignatureTypesList.Add(0x11, "Persona certification of a User ID and Public-Key packet");
            SignatureTypesList.Add(0x12, "Casual certification of a User ID and Public-Key packet");
            SignatureTypesList.Add(0x13, "Positive certification of a User ID and Public-Key packet");
            SignatureTypesList.Add(0x18, "Subkey Binding Signature");
            SignatureTypesList.Add(0x19, "Primary Key Binding Signature");
            SignatureTypesList.Add(0x1F, "Signature directly on a key");
            SignatureTypesList.Add(0x20, "Key revocation signature");
            SignatureTypesList.Add(0x28, "Subkey revocation signature");
            SignatureTypesList.Add(0x30, "Certification revocation signature");
            SignatureTypesList.Add(0x40, "Timestamp signature");
            SignatureTypesList.Add(0x50, "Third-Party Confirmation signature");
        }

        public static string Get(byte Code)
        {
            if (SignatureTypesList.TryGetValue(Code, out string value))
                return value;
            return "Unknown Signature Type";
        }

    }
}
