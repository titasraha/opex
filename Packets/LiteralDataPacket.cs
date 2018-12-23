using System;
using System.IO;
using System.Windows.Forms;
namespace OpenPGPExplorer
{
    public class LiteralDataPacket : PGPPacket
    {
        private ByteBlock ThisBlock { get; set; }
        public string ExtractFileName { get; private set; }

        public override void Parse(TreeBuilder tree)
        {
            tree.ReadByte("Data Format", true);
            byte FileNameLength = tree.ReadByte();
            byte[] FileNameBytes = tree.ReadBytes("File Name", FileNameLength, true);

            ExtractFileName = System.Text.Encoding.UTF8.GetString(FileNameBytes);

            tree.ReadFormatted("Date/Time", BlockFormat.UnixTime);                    
            tree.RemainingBytes("Literal Data");
      
            ThisBlock = tree.CurrentBlock;
            ThisBlock.ProcessBlock += ExtractData;
        }

        private void ExtractData()
        {
            Extract(FileName);
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

        public void Extract(string SourceFileName)
        {

            SaveFileDialog SaveDialog = new SaveFileDialog
            {
                OverwritePrompt = true,
                FileName = ExtractFileName
            };

            if (SaveDialog.ShowDialog() != DialogResult.OK)
                return;

            string DestFileName = SaveDialog.FileName;

            using (PGPReader fsSource = new PGPReader(SourceFileName))
            {
                fsSource.DoIndexUpdate += OnIndexUpdate;
                fsSource.GetPacket(PacketIndex);
                fsSource.DoIndexUpdate -= OnIndexUpdate;

                fsSource.ReadByte();
                int len = fsSource.ReadByte();
                byte[] buf = new byte[256];

                fsSource.Read(buf, 0, len);
                fsSource.Read(buf, 0, 4); // unix time

                using (FileStream fsDest = new FileStream(DestFileName, FileMode.Create, FileAccess.Write))
                {

                    byte[] DataBytes = new byte[4096];
                    var BytesRemaining = fsSource.BytesRemaining;
                    var TotalBytes = BytesRemaining;
                    long CurrentTick = DateTime.Now.Ticks;
                    while (BytesRemaining > 0)
                    {
                        int ReadBytes = (int)BytesRemaining;
                        if (BytesRemaining > DataBytes.Length)
                            ReadBytes = DataBytes.Length;

                        fsSource.Read(DataBytes, 0, ReadBytes);
                        fsDest.Write(DataBytes, 0, ReadBytes);

                        BytesRemaining -= ReadBytes;
                        if (CurrentTick + 10000000 < DateTime.Now.Ticks)
                        {
                            CurrentTick = DateTime.Now.Ticks;
                            OnStatusUpdate("Saving...", (int)(100 * (TotalBytes - BytesRemaining) / TotalBytes));
                        }

                    }

                }
            }
        }
    }
}
