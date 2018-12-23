using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPGPExplorer
{
    public static class PacketTypes
    {
        private static readonly Dictionary<byte, string> PacketTypesList = new Dictionary<byte, string>();
        static PacketTypes()
        {
            PacketTypesList.Add(0, "Reserved - a packet tag MUST NOT have this value");
            PacketTypesList.Add(1, "Public-Key Encrypted Session Key Packet");
            PacketTypesList.Add(2, "Signature Packet");
            PacketTypesList.Add(3, "Symmetric-Key Encrypted Session Key Packet");
            PacketTypesList.Add(4, "One-Pass Signature Packet");
            PacketTypesList.Add(5, "Secret-Key Packet");
            PacketTypesList.Add(6, "Public-Key Packet");
            PacketTypesList.Add(7, "Secret-Subkey Packet");
            PacketTypesList.Add(8, "Compressed Data Packet");
            PacketTypesList.Add(9, "Symmetrically Encrypted Data Packet");
            PacketTypesList.Add(10, "Marker Packet");
            PacketTypesList.Add(11, "Literal Data Packet");
            PacketTypesList.Add(12, "Trust Packet");
            PacketTypesList.Add(13, "User ID Packet");
            PacketTypesList.Add(14, "Public-Subkey Packet");
            PacketTypesList.Add(17, "User Attribute Packet");
            PacketTypesList.Add(18, "Sym. Encrypted and Integrity Protected Data Packet");
            PacketTypesList.Add(19, "Modification Detection Code Packet");
            PacketTypesList.Add(60, "Private or Experimental Values");
            PacketTypesList.Add(61, "Private or Experimental Values");
            PacketTypesList.Add(62, "Private or Experimental Values");
            PacketTypesList.Add(63, "Private or Experimental Values");
        }

        public static string Get(byte Code)
        {
            if (PacketTypesList.TryGetValue(Code, out string value))
                return value;
            return "Unknown Packet Type";
        }

    }
}
