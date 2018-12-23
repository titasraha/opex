using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace OpenPGPExplorer
{
    public class Adler32 : HashAlgorithm
    {
        private ushort o_sum_1;
        private ushort o_sum_2;

        public Adler32()
        {
            Initialize();
        }

        public override int HashSize
        {
            get { return 32; }
        }

        public override void Initialize()
        {
            o_sum_1 = 1;
            o_sum_2 = 0;
        }

        protected override void HashCore(byte[] p_array, int p_start_index, int p_count)
        {         
            for (int i = p_start_index; i < p_count; i++)
            {
                o_sum_1 = (ushort)((o_sum_1 + p_array[i]) % 65521);
                o_sum_2 = (ushort)((o_sum_1 + o_sum_2) % 65521);
            }
        }

        protected override byte[] HashFinal()
        {
            uint x_concat_value = (uint)((o_sum_2 << 16) | o_sum_1);
            return BitConverter.GetBytes(x_concat_value);
        }
    }
}
