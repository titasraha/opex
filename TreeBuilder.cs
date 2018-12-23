using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OpenPGPExplorer
{
    public class TreeBuilder
    {
        private PGPReader _fs;
        private ByteBlock _StartBlock;
        private Stack<ByteBlock> _Level;
        private bool AddChildLevel { get; set; }

        public ByteBlock StartBlock {
            get
            {
                return _StartBlock.NextBlock;
            }
        }
        public ByteBlock CurrentBlock { get; private set; }

        public PGPReader BaseReader
        {
            get
            {
                return _fs;
            }
        }

        public TreeBuilder(PGPReader fs)
        {
            _fs = fs;
            _StartBlock = new ByteBlock();
            CurrentBlock = _StartBlock;
            _Level = new Stack<ByteBlock>();
            AddChildLevel = false;
        }

        public void PushByteBlock()
        {
            _Level.Push(CurrentBlock);
            AddChildLevel = true;
        }

        public void PopByteBlock()
        {
            if (_Level.Count > 0)
                CurrentBlock = _Level.Pop();
            AddChildLevel = false;
        }

        private ByteBlock AddBlock(string Label, string Description, long PositionFromCurrent, byte[] Data)
        {
            if (AddChildLevel)
                CurrentBlock = CurrentBlock.AddChildBlock(Label, Description, _fs.AbsPosition + PositionFromCurrent, Data);
            else
                CurrentBlock = CurrentBlock.AddBlock(Label, Description, _fs.AbsPosition + PositionFromCurrent, Data);
            AddChildLevel = false;
            return CurrentBlock;
        }

        public void SetBookMark()
        {
            _fs.BookMark();
        }

        public void GoToBookMark()
        {
            _fs.GoToBookMark();
        }

        //public void Seek(long FromCurrentPosition)
        //{
        //    _fs.Seek(FromCurrentPosition, SeekOrigin.Current);
        //}

        public ByteBlock AddChild(ByteBlock ChildBlock)
        {
            return CurrentBlock.AddChildBlock(ChildBlock);
        }

        public void AddNode(string Label)
        {            
            AddBlock(Label, "", 0, new byte[] { });
        }

        public uint ReadNumber(int ByteCount)
        {
            return ReadNumber("", ByteCount);
        }

        public uint ReadNumber(string Label, int ByteCount)
        {
            byte[] Data = new byte[ByteCount];

            _fs.Read(Data, 0, ByteCount);
            uint Number = Program.GetBigEndian(Data, 0, ByteCount);

            if (!string.IsNullOrEmpty(Label))
                AddBlock(Label, Number.ToString(), -ByteCount, Data);

            return Number;
        }

        public byte ReadByte()
        {            
            return ReadByte("");
        }

        public byte ReadByte(string Label)
        {            
            return ReadByte(Label, false);
        }

        public byte ReadByte(string Label, bool ReadAsUTF8)
        {            
            int byteValue = _fs.ReadByte();

            if (!string.IsNullOrEmpty(Label))
            {
                string Desc;
                if (ReadAsUTF8)
                    Desc = Encoding.UTF8.GetString(new byte[] { (byte)byteValue });
                else
                    Desc = byteValue.ToString();

                AddBlock(Label, Desc, -1, new byte[] { (byte)byteValue });
            }

            return (byte)byteValue;
        }

        public byte ReadByte(string Label, Func<byte, string> DescriptionFunc)
        {
            int byteValue = _fs.ReadByte();
            if (byteValue >= 0)
            {
                string Description = DescriptionFunc((byte)byteValue);
                AddBlock(Label, Description, -1, new byte[] { (byte)byteValue });
            }
            return (byte)byteValue;
        }

        public byte[] ReadBytes(string Label, int ByteCount)
        {
            return ReadBytes(Label, ByteCount, false);            
        }

        public byte[] ReadBytes(string Label)
        {
            return ReadBytes(Label, false);
        }

        public byte[] ReadBytes(string Label, int ByteCount, bool ReadAsUTF8)
        {
            byte[] Data = new byte[ByteCount];

            _fs.Read(Data, 0, ByteCount);

            string Desc = ByteCount.ToString() + " Bytes";
            if (ReadAsUTF8)
                Desc = Encoding.UTF8.GetString(Data);

            AddBlock(Label, Desc, -ByteCount, Data);

            return Data;

        }

        public byte[] ReadBytes(string Label, bool ReadAsUTF8)
        {
            long remainder = _fs.BytesRemaining;// _fs.NextIdx - _fs.Position;
            return ReadBytes(Label, (int)remainder, ReadAsUTF8);
        }

        public bool IsMoreData()
        {
            return _fs.BytesRemaining > 0;
        }

        //public bool SkipPartialBytes(string Label)
        //{

        //    long PartialBytes = _NextPartialIdx - _fs.Position;
        //    string Desc = PartialBytes.ToString() + " Bytes Skipped";
        //    CurrentBlock = CurrentBlock.AddBlock(Label, Desc, _fs.Position, new byte[] { });
        //    _fs.Seek(_NextPartialIdx, SeekOrigin.Begin);

        //    if (IsMoreData())
        //        _NextPartialIdx = GetNextIdx(_fs);

        //    return IsMoreData();
        //}

        public ByteBlock RemainingBytes(string Label)
        {
            
            long CurrentPosition = _fs.AbsPosition;
            string Desc = _fs.BytesRemaining.ToString() + " Bytes";
            
            return AddBlock(Label, Desc, CurrentPosition, new byte[] { });
        }

        public ByteBlock SkipBytes(string Label)
        {
            return SkipBytes(Label, -1);
        }

        public ByteBlock SkipBytes(string Label, int ByteCount)
        {
            long BytesToSkip = ByteCount;

            if (ByteCount == -1)
                BytesToSkip = _fs.BytesRemaining;// _fs.NextIdx - _fs.Position;

            long CurrentPosition = _fs.AbsPosition;
            string Desc = BytesToSkip.ToString() + " Bytes Skipped";
            //_fs.Seek(_fs.Position + BytesToSkip, SeekOrigin.Begin);
            _fs.Skip(BytesToSkip);

            return AddBlock(Label, Desc, CurrentPosition, new byte[] { });
            
        }

        //public byte[] ReadBytesFromCopyMark()
        //{
        //    // Save Current Position
        //    long Position = _fs.Position;
        //    long ByteCount = Position - _mark;

        //    _fs.Seek(_mark, SeekOrigin.Begin);

        //    byte[] Data = new byte[ByteCount];

        //    _fs.Read(Data, 0, (int)ByteCount);

        //    _fs.Seek(Position, SeekOrigin.Begin);          

        //    return Data;
        //}

        public byte[] ReadBytesFromBookMark()
        {
            return _fs.ReadRawBytesFromBookMark();
        }

        public byte[] ReadFormatted(string Label, BlockFormat format)
        {
            byte[] Data = new byte[4];
            _fs.Read(Data, 0, 4);


            if (format == BlockFormat.UnixTime)
            {
                uint UnixTime = Program.GetBigEndian(Data, 0, 4);

                if (UnixTime == 0)
                    AddBlock(Label, "", -4, Data);
                else
                    AddBlock(Label, Program.UnixTimeStampToDateTime(UnixTime).ToString(), -4, Data);
            }
            else if (format == BlockFormat.Days)
            {

                var v = Program.GetBigEndian(Data, 0, 4);
                TimeSpan t = TimeSpan.FromSeconds(v);

                AddBlock(Label, string.Format("{0:D2}d {1:D2}h:{2:D2}m:{3:D2}s:{4:D3}ms", t.Days, t.Hours, t.Minutes, t.Seconds, t.Milliseconds), -4, Data);
            }
            return Data;
        }

        public byte[] ReadMPIBytes()
        {
            return ReadMPIBytes("");
        }



        public byte[] ReadMPIBytes(string Label)
        {
            byte[] prefix = new byte[2];

            prefix[0] = (byte)_fs.ReadByte();
            prefix[1] = (byte)_fs.ReadByte();

            uint LengthBits = Program.GetBigEndian(prefix, 0, 2);
            int bytes = (int)Math.Ceiling(LengthBits / 8.0);

            byte[] data = new byte[bytes];

            _fs.Read(data, 0, bytes);

            if (!string.IsNullOrEmpty(Label))
                AddBlock(Label, bytes.ToString() + " Bytes", -bytes, data);

            return data;
        }

        public void AddCalculated(string Label, string Description, byte[] Data)
        {
            CurrentBlock = CurrentBlock.AddBlock(Label, Description, -1, Data, ByteBlockType.Calculated);
        }


        public void AddCalculated(string Label, string Description = "", ByteBlockType Type = ByteBlockType.Normal)
        {
            CurrentBlock = CurrentBlock.AddBlock(Label, Description, -1, new byte[] { }, Type);
        }

        public void AddError(string Label, string Description = "")
        {
            CurrentBlock = CurrentBlock.AddBlock(Label, Description, -1, new byte[] { }, ByteBlockType.CalculatedError);
        }



    }

    public enum BlockFormat
    {
        UnixTime = 1,
        Days = 2
    }

}
