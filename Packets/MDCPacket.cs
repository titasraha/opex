using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPGPExplorer
{
    public class MDCPacket : PGPPacket
    {
        public override void Parse(TreeBuilder tree)
        {
            tree.ReadBytes("SHA-1", 20);
        }
    }
}
