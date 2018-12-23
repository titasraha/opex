using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPGPExplorer
{
    static class Program
    {
        public static readonly int MAX_PACKET_LENGTH = 10000000;
        public static readonly int MAX_ARMORED_LENGTH = 10000000;

        


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
            TempFiles.Clear();
        }


        public static long GetCRC24(byte[] octets)
        {
            long crc = 0xB704CEL;
            int len = octets.Length;
            int i;
            while (len-- > 0)
            {
                crc ^= octets[octets.Length - len - 1] << 16;
                for (i = 0; i < 8; i++)
                {
                    crc <<= 1;
                    if ((crc & 0x1000000) != 0)
                        crc ^= 0x1864CFBL;
                }
            }
            return crc & 0xFFFFFFL;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static int GetBlockAlignRemainder(int DataSize, int BlockSize)
        {
            if (DataSize % BlockSize == 0)
                return 0;

            return BlockSize - (DataSize % BlockSize);
        }

        public static string ToHexString(this byte b)
        {
            return "0x" +b.ToString("X2");
            
        }

        public static byte[] GetMPIBytes(byte[] Buffer, ref int Idx)
        {
            uint LengthBits = Program.GetBigEndian(Buffer, Idx, 2);
            int bytes = (int)Math.Ceiling(LengthBits / 8.0);

            Idx += 2;
            byte[] data = new byte[bytes];

            Array.Copy(Buffer, Idx, data, 0, bytes);

            Idx += bytes;

            return data;

        }

        public static uint GetBigEndian(byte[] DataBytes, int idx, int LengthBytes)
        {
            uint length = 0;
            for (int i = idx; i < idx + LengthBytes; i++)
            {
                length |= DataBytes[i];
                if (i < idx + LengthBytes - 1)
                    length <<= 8;
            }
            return length;
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }


}
