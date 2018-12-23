using System;
using System.Security.Cryptography;

namespace OpenPGPExplorer
{
    
    public abstract class PGPPacket: IHashable
    {
        public byte PacketTag { get; set; }
        public long Length { get; set; }
        public int PacketIndex { get; set; }
        public string FileName { get; set; }

        public event StatusUpdates ProgressUpdate;

        public abstract void Parse(TreeBuilder tree);

        public void DoHash(PGPReader Reader, HashAlgorithm[] HashAlgorithms)
        {
            long TotalBytes = Reader.BytesRemaining;
            long RemainingBytes = TotalBytes;
            byte[] Buffer = new byte[4096];
            long CurrentTick = DateTime.Now.Ticks;

            while (RemainingBytes > 0)
            {
                int BytesToRead = (int)RemainingBytes;
                if (BytesToRead > Buffer.Length)
                    BytesToRead = Buffer.Length;

                int BytesRead = Reader.Read(Buffer, 0, BytesToRead);

                foreach (var HashAlgo in HashAlgorithms)
                    HashAlgo.TransformBlock(Buffer, 0, BytesRead, Buffer, 0);
                
                if (CurrentTick + 10000000 < DateTime.Now.Ticks)
                {
                    CurrentTick = DateTime.Now.Ticks;
                    ProgressUpdate?.Invoke("Computing Hash...", (int)(100 * (TotalBytes - RemainingBytes) / TotalBytes));
                }
                RemainingBytes -= BytesRead;
            }
        }
    }

    public interface IHashable
    {
        event StatusUpdates ProgressUpdate;
        void DoHash(PGPReader Reader, HashAlgorithm[] HashAlgorithms);
    }
}
