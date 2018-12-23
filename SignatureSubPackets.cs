using System;

namespace OpenPGPExplorer
{
    public class SignatureSubPackets
    {

        public byte[] Issuer { get; private set; }

        public SignatureSubPackets()
        {
            Issuer = new byte[] { };
        }

        private void ReadByteTags(TreeBuilder tree, string Label, int DataLength, Func<byte,string> DescFunc)
        {
            tree.AddNode(Label);
            tree.PushByteBlock();
            //tree.AddChildLevel = true;
            while (DataLength-- > 0)
            {
                tree.SetBookMark();
                byte ByteCode = tree.ReadByte();
                tree.GoToBookMark();
                //tree.Seek(-1);
                tree.ReadByte(DescFunc(ByteCode));
            }
            tree.PopByteBlock();
        }

        public void Parse(TreeBuilder tree, int SubPacketsLength)
        {

            long SubPacketsEnd = tree.BaseReader.Position + SubPacketsLength;

            tree.PushByteBlock();

            //if (tree.Position < SubPacketsEnd)
            //    tree.AddChildLevel = true;

            while (tree.BaseReader.Position < SubPacketsEnd)
            {
                byte LengthFinder = tree.ReadByte();
                uint SubPacketLength = 0;

                if (LengthFinder < 192)
                    SubPacketLength = LengthFinder;
                else if (LengthFinder < 255)
                {
                    byte b = tree.ReadByte();
                    SubPacketLength = (uint)((LengthFinder - 192) << 8) + b + 192;
                }
                else
                {
                    byte[] b = new byte[4];
                    b[0] = tree.ReadByte();
                    b[1] = tree.ReadByte();
                    b[2] = tree.ReadByte();
                    b[3] = tree.ReadByte();
                    SubPacketLength = Program.GetBigEndian(b, 0, 4);
                }

                byte SubPacketType = tree.ReadByte();
                string Label = SignatureSubPacketTypes.Get(SubPacketType);

                int DataLength = (int)SubPacketLength - 1;

                if (SubPacketType == SignatureSubPacketTypes.SignatureCreationTime)
                    tree.ReadFormatted(Label, BlockFormat.UnixTime);
                else if (SubPacketType == SignatureSubPacketTypes.Issuer)
                    Issuer = tree.ReadBytes(Label, 8);
                else if (SubPacketType == SignatureSubPacketTypes.KeyExpirationTime)
                    tree.ReadFormatted(Label, BlockFormat.Days);
                else if (SubPacketType == SignatureSubPacketTypes.PreferredSymmetricAlgorithm)
                    ReadByteTags(tree, Label, DataLength, SymmetricAlgorithmTypes.Get);
                else if (SubPacketType == SignatureSubPacketTypes.PreferredHashAlgorithm)
                    ReadByteTags(tree, Label, DataLength, HashAlgorithmTypes.Get);
                else if (SubPacketType == SignatureSubPacketTypes.PreferredCompressionAlgorithm)
                    ReadByteTags(tree, Label, DataLength, CompressionAlgorithmTypes.Get);
                else if (SubPacketType == SignatureSubPacketTypes.KeyFlags)
                    ReadByteTags(tree, Label, DataLength, KeyFlagsTypes.Get);
                else if (SubPacketType == SignatureSubPacketTypes.PrimaryUserId)
                    tree.ReadBytes(Label, DataLength);
                else if (SubPacketType == SignatureSubPacketTypes.ReasonForRevocation)
                {
                    tree.AddNode(Label);
                    tree.PushByteBlock();                    
                    tree.ReadByte("Reason", RevocationReasonTypes.Get);
                    tree.ReadBytes("Message", DataLength-1);
                    tree.PopByteBlock();
                }
                else
                    tree.ReadBytes(Label, DataLength);
            }

            tree.PopByteBlock();
        }        

        
    }
}
