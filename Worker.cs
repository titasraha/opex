using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPGPExplorer
{
    public delegate void AddCompleteDelegate(string FileName, ByteBlock StartBlock);
    public delegate void ProcessCompleteDelegate(ByteBlock BB, TreeNode tn);
    public static class Worker
    {

        public static event AddCompleteDelegate AddComplete;
        public static event ProcessCompleteDelegate ProcessComplete;
        public static event StatusUpdates StatusUpdate;
        public static event ErrorDelegate Error;

        private static Thread WorkerThread;

        private class BlockInfo
        {
            public ByteBlock BB;
            public TreeNode tn;
        }

        public static void AddFile(string FileName)
        {
            if (IsProcessing())
                return;

            WorkerThread = new Thread(new ParameterizedThreadStart(AddFile));
            WorkerThread.SetApartmentState(ApartmentState.STA);
            WorkerThread.Start(FileName);
        }

        public static bool ProcessBlock(ByteBlock BB, TreeNode tn)
        {
            if (IsProcessing())
                return false;

            WorkerThread = new Thread(new ParameterizedThreadStart(DoProcessBlock));
            WorkerThread.SetApartmentState(ApartmentState.STA);
            WorkerThread.Start(new BlockInfo { BB = BB, tn = tn });
          
            return true;
        }

        private static bool IsProcessing()
        {
            if (WorkerThread != null)
                if (WorkerThread.IsAlive)
                {
                    Error?.Invoke("Please wait for the process to complete", false);
                    return true;
                }
            return false;
        }

        private static void DoProcessBlock(object BlockInfo)
        {
            try
            {
                var Info = (BlockInfo)BlockInfo;

                
                Info.BB.StatusUpdate += StatusUpdate;
                
                Info.BB.ProcessBlock();
                ProcessComplete?.Invoke(Info.BB, Info.tn);

            }
            catch (Exception ex)
            {
                Error?.Invoke(ex.Message, true);
            }
        }


        private static void AddFile(object OFullFileName)
        {           
            try
            {
                string FullFileName = OFullFileName.ToString();
                var FileName = Path.GetFileName(FullFileName);
                string BinaryGPGFile = GetBinaryFile(FullFileName);

                ByteBlock StartBlock = OpenPGP.Process(BinaryGPGFile);

                OpenPGP.Validate();

                AddComplete?.Invoke(FileName, StartBlock);

            }
            catch (Exception ex)
            {
                Error?.Invoke(ex.Message, true);
            }
        }

        private static string GetBinaryFile(string FileName)
        {

            using (FileStream sr = new FileStream(FileName, FileMode.Open, FileAccess.Read))
            {
                int FirstByte = sr.ReadByte();

                if ((FirstByte & 0x80) != 0)
                    return FileName;
            }



            // Assuming ASCII Armored Text File
            using (StreamReader reader = File.OpenText(FileName))
            {
                string line = reader.ReadLine();
                if (!line.StartsWith("-----"))
                    throw new InvalidDataException("Not a valid Open PGP file");

                FileInfo fi = new FileInfo(FileName);
                if (fi.Length > Program.MAX_ARMORED_LENGTH)
                    throw new InvalidDataException("ASCII-Armored file too big, please convert to binary and try again");

                while (!reader.EndOfStream && !string.IsNullOrWhiteSpace(reader.ReadLine())) ;

                StringBuilder sb = new StringBuilder();
                long CheckSum = 0;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (line.StartsWith("="))
                    {
                        byte[] CheckSumBytes = Convert.FromBase64String(line.Substring(1));

                        CheckSum = Program.GetBigEndian(CheckSumBytes, 0, CheckSumBytes.Length);
                        break;
                    }
                    else if (string.IsNullOrWhiteSpace(line))
                        break;

                    sb.Append(line);
                }

                byte[] DataBytes = Convert.FromBase64String(sb.ToString());
                long CRC = Program.GetCRC24(DataBytes);

                if (CRC != CheckSum)
                    throw new InvalidDataException("ASCII-Armored checksum failed, file may be corrupt");


                string TempFileName = TempFiles.GetNewTempFile();
                File.WriteAllBytes(TempFileName, DataBytes);

                return TempFileName;

            }


        }

    }
}
