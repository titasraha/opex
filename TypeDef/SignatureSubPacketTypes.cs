using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPGPExplorer
{
    public static class SignatureSubPacketTypes
    {
        private static readonly Dictionary<byte, string> SignatureSubPacketList = new Dictionary<byte, string>();
        static SignatureSubPacketTypes()
        {
            SignatureSubPacketList.Add(0, "Reserved");
            SignatureSubPacketList.Add(1, "Reserved");
            SignatureSubPacketList.Add(SignatureCreationTime, "Signature Creation Time");
            SignatureSubPacketList.Add(4, "Exportable Certification");
            SignatureSubPacketList.Add(5, "Trust Signature");
            SignatureSubPacketList.Add(6, "Regular Expression");
            SignatureSubPacketList.Add(7, "Revocable");
            SignatureSubPacketList.Add(8, "Reserved");
            SignatureSubPacketList.Add(KeyExpirationTime, "Key Expiration Time");
            SignatureSubPacketList.Add(10, "For Backward Compatibility");
            SignatureSubPacketList.Add(PreferredSymmetricAlgorithm, "Preferred Symmetric Algorithm");
            SignatureSubPacketList.Add(RevocationKey, "Revocation Key");
            SignatureSubPacketList.Add(13, "Reserved");
            SignatureSubPacketList.Add(14, "Reserved");
            SignatureSubPacketList.Add(15, "Reserved");
            SignatureSubPacketList.Add(Issuer, "Issuer");
            SignatureSubPacketList.Add(17, "Reserved");
            SignatureSubPacketList.Add(18, "Reserved");
            SignatureSubPacketList.Add(19, "Reserved");
            SignatureSubPacketList.Add(20, "Notification Data");
            SignatureSubPacketList.Add(PreferredHashAlgorithm, "Preferred Hash Algorithm");
            SignatureSubPacketList.Add(PreferredCompressionAlgorithm, "Preferred Compression Algorithm");
            SignatureSubPacketList.Add(23, "Key Server Preferences");
            SignatureSubPacketList.Add(24, "Preferred Key Server");
            SignatureSubPacketList.Add(PrimaryUserId, "Primary User ID");
            SignatureSubPacketList.Add(26, "Policy URI");
            SignatureSubPacketList.Add(KeyFlags, "Key Flags");
            SignatureSubPacketList.Add(28, "Signer's User ID");
            SignatureSubPacketList.Add(ReasonForRevocation, "Reason for Revocation");
            SignatureSubPacketList.Add(30, "Features");
            SignatureSubPacketList.Add(31, "Signature Target");
            SignatureSubPacketList.Add(32, "Embedded Signature");

            
        }

        public static string Get(byte Code)
        {
            if (SignatureSubPacketList.TryGetValue(Code, out string value))
                return value;
            return "Unknown Sub Packet (" + Code.ToString() + ")" ;
        }

        public static readonly byte SignatureCreationTime = 2;
        public static readonly byte KeyExpirationTime = 9;
        public static readonly byte PreferredSymmetricAlgorithm = 11;
        public static readonly byte RevocationKey = 12;
        public static readonly byte Issuer = 16;
        public static readonly byte PrimaryUserId = 25;
        public static readonly byte PreferredHashAlgorithm = 21;
        public static readonly byte PreferredCompressionAlgorithm = 22;
        public static readonly byte KeyFlags = 27;
        public static readonly byte ReasonForRevocation = 29;

    }
}
