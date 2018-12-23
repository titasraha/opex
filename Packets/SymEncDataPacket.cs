namespace OpenPGPExplorer
{
    public class SymEncDataPacket : PGPPacket
    {
        public byte Version { private set; get; }

        public override void Parse(TreeBuilder tree)
        {
            tree.SkipBytes("Encrypted Data");
        }
    }
}
