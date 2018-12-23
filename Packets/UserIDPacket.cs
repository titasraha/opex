using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPGPExplorer
{
    public class UserIDPacket : PGPPacket
    {
        public string UserID
        {
            get
            {
                return Encoding.UTF8.GetString(UserIDBytes);
            }
        }
        public byte[] UserIDBytes { get; private set; }

        public override void Parse(TreeBuilder tree)
        {
            UserIDBytes = tree.ReadBytes("User Id", true);            
        }
    }
}
