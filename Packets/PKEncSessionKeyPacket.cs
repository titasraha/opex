using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPGPExplorer
{
    public class PKEncSessionKeyPacket : PGPPacket
    {
        public byte Version { private set; get; }
        public byte[] KeyId { private set; get; }
        public byte PKAlgoCode { private set; get; }
        //public PKAlgorithm PublicKeyAlgorithm { private set; get; }
        public ITransformedData PublicKeyTransformedData { private set; get; }


        public override void Parse(TreeBuilder tree)
        {
            Version = tree.ReadByte("Version");
            KeyId = tree.ReadBytes("Key Id", 8);
            PKAlgoCode = tree.ReadByte("PK Algorithm", PKAlgorithmTypes.Get);

            var PublicKeyAlgorithm = PKAlgorithm.CreatePKAlgorithm(PKAlgoCode);
            PublicKeyTransformedData = null;
            if (PublicKeyAlgorithm == null)
                tree.ReadBytes("Unknowon Encrypted Session Key");
            else
                PublicKeyTransformedData = PublicKeyAlgorithm.LoadPublicKeyTransformedData(tree);

        }
    }
}
