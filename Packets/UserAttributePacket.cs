namespace OpenPGPExplorer
{
    public class UserAttributePacket : PGPPacket
    {

        public override void Parse(TreeBuilder tree)
        {
            while (tree.IsMoreData())
            {
                byte LengthFinder = tree.ReadByte();
                long SubPacketLength;

                if (LengthFinder < 192)
                    SubPacketLength = LengthFinder;
                else if (LengthFinder <= 223)
                {
                    byte b = tree.ReadByte();
                    SubPacketLength = (uint)((LengthFinder - 192) << 8) + b + 192;
                }
                else if (LengthFinder < 255)
                {
                    // What to do???
                    tree.AddCalculated("Error", "Unable To Process");
                    return;
                }
                else
                {
                    // To test
                    byte[] b = new byte[4];
                    b[0] = tree.ReadByte();
                    b[1] = tree.ReadByte();
                    b[2] = tree.ReadByte();
                    b[3] = tree.ReadByte();
                    SubPacketLength = Program.GetBigEndian(b, 0, 4);
                }

                byte SubPacketType = tree.ReadByte("Sub Packet Type");
                tree.SkipBytes("Sub Packet", (int)SubPacketLength);
            }
        }
    }
}
