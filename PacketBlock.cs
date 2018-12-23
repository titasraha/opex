using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPGPExplorer
{
    
    public class PacketBlock : ByteBlock
    {
        public PGPPacket PGPPacket { get; private set; }
        public string Message { get; set; }
   

        public PacketBlock(PGPPacket pgp)
        {
            Label = PacketTypes.Get(pgp.PacketTag) + " (" + pgp.PacketTag.ToString() + ")";
            Description = pgp.Length.ToString() + " Bytes";
            RawBytes = new byte[] { };
            PGPPacket = pgp;
            
        }

        
    }
}
