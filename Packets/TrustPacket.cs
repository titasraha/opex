namespace OpenPGPExplorer
{
    public class TrustPacket : PGPPacket
    {

        public override void Parse(TreeBuilder tree)
        {
            tree.AddCalculated("Trust Packet", "Not Implemented");

        }
    }
}
