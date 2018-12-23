namespace OpenPGPExplorer
{
    public class MarkerPacket : PGPPacket
    {
        public override void Parse(TreeBuilder tree)
        {
            tree.ReadBytes("Marker Packet");
            tree.AddCalculated("Obsolete Literal Packet", "Do Not Use");
        }
    }
}
