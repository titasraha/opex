using System;
using System.IO;


namespace OpenPGPExplorer
{
    public class PGPStream : Stream
    {
        private PGPReader _PGPReader;
        private long _SkipEndBytes;
        private long _Length;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => _Length;

        public override long Position { get => _PGPReader.Position; set => throw new NotImplementedException(); }

        public PGPStream(PGPReader Reader, long SkipEndBytes)
        {
            _PGPReader = Reader;
            _SkipEndBytes = SkipEndBytes;
            _Length =  _PGPReader.Length - _SkipEndBytes;
        }

        
        public override void Flush()
        {
            
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int ReadableBytes = (int)(_PGPReader.BytesRemaining - _SkipEndBytes);

            if (count > ReadableBytes)
                count = ReadableBytes;

            return _PGPReader.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException();
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException();
        }
    }
}
