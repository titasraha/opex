using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPGPExplorer
{
    public delegate void ByteBlockProcess();
    public delegate void StatusUpdates(string Message, int Percent = -1);
    public delegate void ErrorDelegate(string Message, bool Reset);
    public class ByteBlock
    {
        public string Label { get; set; }
        public string Description { get; set; }
        public byte[] RawBytes { get; set; }
        public long Position { get; set; }

        public ByteBlockType Type { get; set; }

        public ByteBlock NextBlock { get; private set; }
        public ByteBlock ChildBlock { get; private set; }

        public ByteBlockProcess ProcessBlock;
        public StatusUpdates StatusUpdate;


        public ByteBlock()
        {
            
        }

        //public void ClearMessageBlocks()
        //{
        //    var Next = NextBlock;
        //    var PrevBlock = NextBlock;
        //    while(Next != null)
        //    {
        //        if (Next.Type == ByteBlockType.MessageBlock)
        //            PrevBlock.NextBlock = Next.NextBlock;
        //        else
        //            PrevBlock = Next.NextBlock;
        //        Next = PrevBlock;
        //    }
        //}
        
        public ByteBlock(string Label, string Description, long Position, byte[] RawBytes)
        {
            this.Label = Label;
            this.Description = Description;
            this.Position = Position;
            this.RawBytes = RawBytes;

        }

        public ByteBlock AddBlock(ByteBlock NewBlock)
        {
            var LastBlock = NextBlock;
            var PreviousBlock = NextBlock;

            while (LastBlock != null)
            {
                PreviousBlock = LastBlock;
                LastBlock = LastBlock.NextBlock;
            }
            if (PreviousBlock == null)
                NextBlock = NewBlock;
            else
                PreviousBlock.NextBlock = NewBlock;

            return NewBlock;
        }


        public ByteBlock AddBlock(string Label, string Description, long Position, byte[] RawBytes, ByteBlockType Type = ByteBlockType.Normal)
        {
            var NewBlock = new ByteBlock(Label, Description, Position, RawBytes);
            NewBlock.Type = Type;

            return AddBlock(NewBlock);
            
        }


        public ByteBlock AddChildBlock(string Label, string Description, long Position, byte[] RawBytes)
        {
            var NewBlock = new ByteBlock(Label, Description, Position, RawBytes);
            return AddChildBlock(NewBlock);
        }

        public ByteBlock AddChildBlock(ByteBlock block)
        {
            var LastBlock = ChildBlock;
            var PreviousBlock = ChildBlock;

            while (LastBlock != null)
            {
                PreviousBlock = LastBlock;
                LastBlock = LastBlock.NextBlock;
            }
            if (PreviousBlock == null)
                ChildBlock = block;
            else
                PreviousBlock.NextBlock = block;
            return block;
        }
    }

    public enum ByteBlockType
    {
        Normal, TooBig, Calculated, CalculatedError, CalculatedSuccess
    }
}
