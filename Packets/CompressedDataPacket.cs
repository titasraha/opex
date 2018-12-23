using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using ICSharpCode.SharpZipLib.BZip2;

namespace OpenPGPExplorer
{
    public class CompressedDataPacket : PGPPacket
    {
       
        private ByteBlock ThisBlock { get; set; }

        public override void Parse(TreeBuilder tree)
        {
            tree.ReadByte("Compression Algorithm", CompressionAlgorithmTypes.Get);
            tree.SkipBytes("Compressed Data");
            
            ThisBlock = tree.CurrentBlock;
            ThisBlock.ProcessBlock += ExtractData;

        }

        private void OnIndexUpdate(int Ctr)
        {
            string Msg = "Building Index " + Ctr.ToString();
            ThisBlock.StatusUpdate?.Invoke(Msg);
        }

        private void OnStatusUpdate(string Msg, int Percent)
        {
            ThisBlock.StatusUpdate?.Invoke(Msg, Percent);
        }

        private void ExtractData()
        {
            if (System.Windows.Forms.MessageBox.Show("Extract Compressed Data", "Please Confirm", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
                return;
            Extract(FileName);
        }

        private void CopyStream(Stream Base, Stream Source, Stream Dest, HashAlgorithm HashAlgo = null)
        {
            byte[] Buffer = new byte[4096];
            long FileSize = Base.Length;
            int BytesRead;

            do
            { 
                BytesRead = Source.Read(Buffer, 0, 4096);
                Dest.Write(Buffer, 0, BytesRead);
                if (HashAlgo != null)
                    HashAlgo.TransformBlock(Buffer, 0, BytesRead, Buffer, 0);
                OnStatusUpdate("Extracting...", (int)(100 * Base.Position / FileSize));
            } while (BytesRead > 0);

            if (HashAlgo != null)
                HashAlgo.TransformFinalBlock(Buffer, 0, 0);
            
        }

        public void Extract(string SourceFileName)
        {

            string DestFileName;

            using (PGPReader fsSource = new PGPReader(SourceFileName))
            {
                fsSource.DoIndexUpdate += OnIndexUpdate;
                fsSource.GetPacket(PacketIndex);
                fsSource.DoIndexUpdate -= OnIndexUpdate;

                byte CompressionAlgorithm = fsSource.ReadByte();

                if (CompressionAlgorithm != 0 && CompressionAlgorithm != 1 && CompressionAlgorithm != 2 && CompressionAlgorithm != 3)
                    throw new Exception("Compression algorithm not supported");


                int HashBytes = 0;
                if (CompressionAlgorithm == 2)
                {
                    // RFC 1950
                    HashBytes = 4;
                    byte CMF = fsSource.ReadByte();
                    byte FLG = fsSource.ReadByte();
                    if ((FLG & 32) != 0)
                    {
                        byte[] DICT = new byte[4];
                        fsSource.Read(DICT, 0, 4);
                    }
                }

               
                Adler32 adler32 = new Adler32();
                byte[] Hash = null;
                
                PGPStream PGPStream = new PGPStream(fsSource, HashBytes);
                
                DestFileName = TempFiles.GetNewTempFile();
                using (FileStream fsDest = new FileStream(DestFileName, FileMode.Create, FileAccess.Write))
                {
                    if (CompressionAlgorithm == 0)
                        CopyStream(PGPStream, PGPStream, fsDest);
                    else if (CompressionAlgorithm == 3)
                    {
                        using (var BZip2Stream = new BZip2InputStream(PGPStream))
                        {
                            CopyStream(PGPStream, BZip2Stream, fsDest);
                        }
                    }
                    else
                        using (DeflateStream decompressionStream = new DeflateStream(PGPStream, CompressionMode.Decompress))
                        {
                            CopyStream(PGPStream, decompressionStream, fsDest, adler32);
                            Hash = adler32.Hash;
                        }
                }
                
                
                if (HashBytes > 0)
                {
                    byte[] HashCompare = new byte[4];
                    fsSource.Read(HashCompare, 0, 4);

                    if (!HashCompare.Reverse().SequenceEqual(Hash))
                        throw new Exception("Decompressed Hash Mismatch");
                }
            }

            ByteBlock StartBlock = OpenPGP.Process(DestFileName);
            OpenPGP.Validate();
            ThisBlock.AddChildBlock(StartBlock);            
        }
    }
}
