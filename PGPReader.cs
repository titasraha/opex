using System;
using System.Collections.Generic;
using System.IO;


namespace OpenPGPExplorer
{
    public delegate void IndexUpdate(int Ctr);
    public partial class PGPReader:IDisposable
    {
        private FileStream _fs;
        private long _FileLength;

        private long _Idx;          // Absolute byte position of the start of the current packet in the file
        private long _NextIdx;      // Absolute byte position of the start of the next packet in the file

        private int _PacketIndex;   // Packet # in the file (Zero based index)
        private int _PacketOffset;  // Bytes of metadata to skip for actual data

        private long _BookMark;
        private long[] _Partials;
       

        public string FileName { get; }

        public event IndexUpdate DoIndexUpdate;

        public int PacketId
        {
            get
            {
                return _PacketIndex;
            }
        }

        public long Length { get; private set; }
        
        public long BytesRemaining
        {
            get
            {
                int LengthIdx = Array.BinarySearch(_Partials, _fs.Position);                

                if (LengthIdx < 0)
                    LengthIdx = ~LengthIdx;

                var ExtraBytes = _Partials.Length - LengthIdx;

                return _NextIdx - _fs.Position - ExtraBytes;
            }
        }

        public long AbsPosition
        {
            get
            {
                return _fs.Position;
            }
        }

        public long Position
        {
            get
            {
                if (_PacketIndex < 0)
                    throw new InvalidOperationException("Not a valid Packet");


                int LengthIdx = Array.BinarySearch(_Partials, _fs.Position);

                if (LengthIdx < 0)
                    LengthIdx = ~LengthIdx;

                return _fs.Position - _Idx - LengthIdx - _PacketOffset;

            }
        }

        //public void Seek(long Position)
        //{
        //    if (Position < 0)
        //        throw new IndexOutOfRangeException("Position Can not be negative");

        //    // TODO fix for partials
        //    _fs.Seek(_Idx + Position, SeekOrigin.Begin);
        //}

        public PGPReader(string FileName)
        {

            _fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            _FileLength = _fs.Length;
            this.FileName = _fs.Name;
            Reset();
        }



        public void BookMark()
        {
            _BookMark = _fs.Position;
        }

        public bool GoToBookMark()
        {
            if (_BookMark >= 0)
            {
                _fs.Seek(_BookMark, SeekOrigin.Begin);
                return true;
            }
            return false;
        }

        public byte[] ReadRawBytesFromBookMark()
        {

            long Position = _fs.Position;
            long ByteCount = Position - _BookMark;

            _fs.Seek(_BookMark, SeekOrigin.Begin);

            byte[] Data = new byte[ByteCount];

            _fs.Read(Data, 0, (int)ByteCount);

            _fs.Seek(Position, SeekOrigin.Begin);

            return Data;
        }

        public bool Skip(long BytesToSkip)
        {

            if (BytesRemaining >= BytesToSkip)
            {
                long SkipBytes;
                do
                {
                    long NextPos = GetNextBoundary();
                    var BytesAvailable = NextPos - _fs.Position;

                    if (BytesAvailable > BytesToSkip)
                        SkipBytes = BytesToSkip;
                    else
                        SkipBytes = BytesAvailable;
                        
                    _fs.Seek(SkipBytes, SeekOrigin.Current);
                    

                    BytesToSkip -= SkipBytes;
                } while (BytesToSkip > 0);
                
                return true;
            }
            return false;
        }



        public void Reset()
        {
            _Idx = -1;
            _NextIdx = 0;
            _PacketIndex = -1;
            _BookMark = -1;
        }

        private long GetNextBoundary()
        {
            int LengthIdx = Array.BinarySearch(_Partials, _fs.Position);
            int CompareIdx = LengthIdx;

            if (LengthIdx < 0)
                CompareIdx = ~LengthIdx;
            
            if (CompareIdx >= _Partials.Length)
                return _NextIdx;

            if (LengthIdx >= 0)
            {
                // Position falls on the meta byte
                while (LengthIdx < _Partials.Length && _Partials[LengthIdx] == _fs.Position)
                {
                    _fs.Seek(1, SeekOrigin.Current);
                    LengthIdx++;
                }

                if (LengthIdx < _Partials.Length)
                    return _Partials[LengthIdx];
            }
            else
                if (CompareIdx < _Partials.Length)
                    return _Partials[CompareIdx];

            return _NextIdx;
        }

        // Abstract reading of partial bytes
        public int Read(byte[] array, int offset, int count)
        {
            int TotalBytesRead = 0;
            int BytesRead = 0;

            do
            {
                long NextPos = GetNextBoundary();
                var BytesAvailable = NextPos - _fs.Position;
                
                if (BytesAvailable > count)
                    BytesRead = _fs.Read(array, offset, count);
                else
                    BytesRead = _fs.Read(array, offset, (int)BytesAvailable);
                offset += BytesRead;
                count -= BytesRead;
                TotalBytesRead += BytesRead;
            } while (count > 0 && BytesRead > 0);

            return TotalBytesRead;
        }

        public byte ReadByte()
        {
            byte[] ByteBuffer = new byte[] { 0x00 };

            Read(ByteBuffer, 0, 1);

            return (byte)ByteBuffer[0];
        }

        public PGPPacket GetPacket(int PacketIdx)
        {
            if (PacketIdx <= _PacketIndex)
                Reset();

            PGPPacket pkt;
            while ((pkt = ReadNextPacket()) != null && PacketIdx > _PacketIndex) ;

            return pkt;
        }

        public PGPPacket ReadNextPacket()
        {
                     
            if (_NextIdx >= _FileLength)
                return null;

            _Idx = _NextIdx;
            _PacketIndex++;
            _BookMark = -1;

            _fs.Seek(_Idx, SeekOrigin.Begin);

            byte PacketTag = (byte)_fs.ReadByte();
            int check = PacketTag & 0x80;

            if (check == 0)
                throw new InvalidOperationException("Invalid OpenPGP Packet");

            check = PacketTag & 0x40;
            if (check == 0)
                return GetOldFormat(PacketTag);
            return GetNewFormat(PacketTag);
        }

        //public bool SeekPacket(int PacketIdx)
        //{
        //    Reset();

        //    while (ReadNextPacket() != null && PacketIdx != _PacketIndex) ;

        //    return (PacketIdx == _PacketIndex);
        //}

        //public byte[] ReadPacketBytes()
        //{
        //    // Todo fix for partial packets
        //    long len = _NextIdx - _Idx;
        //    long OldPos = _fs.Position;

        //    byte[] Buffer = new byte[len];
        //    _fs.Seek(_Idx, SeekOrigin.Begin);
        //    _fs.Read(Buffer, 0, (int)len);
        //    _fs.Seek(OldPos, SeekOrigin.Begin);

        //    return Buffer;

        //}




        private PGPPacket CreatePGPPacket(byte PacketTag)
        {
            PGPPacket PGPPacket;

            if (PacketTag == 1)
                PGPPacket = new PKEncSessionKeyPacket();
            else if (PacketTag == 2)
                PGPPacket = new SignaturePacket();
            else if (PacketTag == 3)
                PGPPacket = new SymEncSessionKeyPacket();
            else if (PacketTag == 4)
                PGPPacket = new OnePassSignaturePacket();
            else if (PacketTag == 5)
                PGPPacket = new SecretKeyPacket();
            else if (PacketTag == 6)
                PGPPacket = new PublicKeyPacket();
            else if (PacketTag == 7)
                PGPPacket = new SecretKeyPacket();
            else if (PacketTag == 8)
                PGPPacket = new CompressedDataPacket();
            else if (PacketTag == 9)
                PGPPacket = new SymEncDataPacket();
            else if (PacketTag == 10)
                PGPPacket = new MarkerPacket();
            else if (PacketTag == 11)
                PGPPacket = new LiteralDataPacket();
            else if (PacketTag == 12)
                PGPPacket = new TrustPacket();
            else if (PacketTag == 13)
                PGPPacket = new UserIDPacket();
            else if (PacketTag == 14)
                PGPPacket = new PublicKeyPacket();
            else if (PacketTag == 17)
                PGPPacket = new UserAttributePacket();
            else if (PacketTag == 18)
                PGPPacket = new SymEncIPDataPacket();
            else if (PacketTag == 19)
                PGPPacket = new MDCPacket();
            else
                PGPPacket = new UnknownPacket();

            Length = BytesRemaining;
            _PacketOffset = (int)( _fs.Position - _Idx);

            PGPPacket.Length = this.Length;
            PGPPacket.PacketTag = PacketTag;
            PGPPacket.PacketIndex = _PacketIndex;
            PGPPacket.FileName = this.FileName;

            return PGPPacket;
        }

        private PGPPacket GetOldFormat(byte HeaderByte)
        {
            uint length = 0;


            byte PacketTag = (byte)(((uint)HeaderByte >> 2) & 0x0f);
            int LengthType = HeaderByte & 0x03;
            int LengthBytes;

            if (LengthType == 0)
                LengthBytes = 1;
            else if (LengthType == 1)
                LengthBytes = 2;
            else if (LengthType == 2)
                LengthBytes = 4;
            else
            {
                length = (uint)(_fs.Length - _fs.Position);
                LengthBytes = 0;
            }

            while (LengthBytes-- > 0)
            {
                length |= (byte)_fs.ReadByte();
                if (LengthBytes > 0)
                    length <<= 8;
            }

            if (_fs.Position + length > _FileLength)
                throw new InvalidOperationException("Invalid length specified");

            _NextIdx = (int)_fs.Position + (int)length;
            _Partials = new long[] { };

            return CreatePGPPacket(PacketTag);
        }

        private PGPPacket GetNewFormat(byte HeaderByte)
        {

            byte PacketTag = (byte)(HeaderByte & 0x3f);
            bool IsPartial = false;

            byte LengthFinder = (byte)_fs.ReadByte();// _DataBytes[Idx];
            long SubPacketLength = 0;
            long PacketDataStartPosition = _fs.Position;
            List<long> PartialList = new List<long>();
            int LengthOfLength = 1;

            long CurrentTick = DateTime.Now.Ticks;
            bool IsProcessed;

            do
            {
                IsProcessed = true;

                if (LengthFinder < 192)
                    SubPacketLength = LengthFinder;
                else if (LengthFinder <= 223)
                {
                    byte b = (byte)_fs.ReadByte();
                    SubPacketLength = (uint)((LengthFinder - 192) << 8) + b + 192;
                    LengthOfLength = 2;
                }
                else if (LengthFinder < 255)
                {
                    long PartialPacketLength = 1 << (LengthFinder & 0x1F);
                    if (!IsPartial)
                    {
                        PacketDataStartPosition = _fs.Position;
                        IsPartial = true;                        
                    }

                    _fs.Seek(PartialPacketLength, SeekOrigin.Current);
                    PartialList.Add(_fs.Position);
                    LengthFinder = (byte)_fs.ReadByte();
                    IsProcessed = false;
                    if (CurrentTick + 2500000 < DateTime.Now.Ticks)
                    {
                        CurrentTick = DateTime.Now.Ticks;
                        DoIndexUpdate?.Invoke(PartialList.Count);
                    }
                }
                else
                {
                    // To test
                    byte[] b = new byte[4];
                    b[0] = (byte)_fs.ReadByte();
                    b[1] = (byte)_fs.ReadByte();
                    b[2] = (byte)_fs.ReadByte();
                    b[3] = (byte)_fs.ReadByte();
                    SubPacketLength = Program.GetBigEndian(b, 0, 4);
                    LengthOfLength = 5;
                }

            } while (!IsProcessed);

            if (_fs.Position + SubPacketLength > _FileLength)
                throw new InvalidOperationException("Invalid length specified");

            _NextIdx = _fs.Position + SubPacketLength;

            if (IsPartial)
            {
                for (int i = 0; i < LengthOfLength - 1; i++)
                    PartialList.Add(_fs.Position - (LengthOfLength - i - 1));
                _fs.Seek(PacketDataStartPosition, SeekOrigin.Begin);
            }

            _Partials = PartialList.ToArray();

            return CreatePGPPacket(PacketTag);

        }

        public void Dispose()
        {
            _fs.Close();
        }
    }

}
