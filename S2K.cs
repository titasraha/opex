using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace OpenPGPExplorer
{
    public class S2K
    {
        public byte S2KUsage { get; set; }
        public byte[] Salt { get; set; }
        public int ByteCount { get; set; }
        public byte HashAlgorithm { get; set; }

        

        public byte SymAlgo { get; set; }
        public byte[] IV { get; set; }

        

        public S2K()
        {
            Salt = new byte[0];
            ByteCount = 0;
        }

        public byte[] GetKey(string Password)
        {
            int KeySize = SymmetricAlgorithmTypes.GetKeySize(SymAlgo);
            int DesiredBytes = (byte)(KeySize / 8);
            HashAlgorithm HashAlgo = HashAlgorithmTypes.GetHashAlgoManaged(HashAlgorithm);
            byte[] Key = GetSaltedIterated(HashAlgo, DesiredBytes, Salt, Encoding.ASCII.GetBytes(Password), ByteCount);
            return Key;
        }



        private byte[] ComputeHash(HashAlgorithm HashAlgo, int InstanceId, byte[] Context, int ByteCount)
        {
            int ContextLength = Context.Length;
            byte[] OutBuffer = new byte[ContextLength];
            int Ctr = ByteCount / ContextLength;

            HashAlgo.Initialize();

            if (InstanceId > 0)
            {
                byte[] ByteBuffer = new byte[1];
                ByteBuffer[0] = (byte)(InstanceId - 1);
                HashAlgo.TransformBlock(ByteBuffer, 0, 1, ByteBuffer, 0);
            }

            while (Ctr-- > 0)
                HashAlgo.TransformBlock(Context, 0, Context.Length, OutBuffer, 0);


            int Remainder = ByteCount % ContextLength;

            HashAlgo.TransformFinalBlock(Context, 0, Remainder);
            return HashAlgo.Hash;
        }

        private byte[] GetSaltedIterated(HashAlgorithm HashAlgo, int DesiredBytes, byte[] Salt, byte[] Password, int ByteCount)
        {
            int SaltPwdLength = Salt.Length + Password.Length;
            //this.ByteCount = ByteCount;

            byte[] IterationContext = new byte[SaltPwdLength];


            Array.Copy(Salt, 0, IterationContext, 0, Salt.Length);
            Array.Copy(Password, 0, IterationContext, Salt.Length, Password.Length);

            if (ByteCount < SaltPwdLength)
                ByteCount = SaltPwdLength;


            byte[] FinalHash = new byte[DesiredBytes];
            int FillIdx = 0;
            int InstanceId = 0;

            do
            {
                byte[] ComputedHash = ComputeHash(HashAlgo, InstanceId++, IterationContext, ByteCount);
                if (FillIdx + ComputedHash.Length > FinalHash.Length)
                {
                    Array.Copy(ComputedHash, 0, FinalHash, FillIdx, FinalHash.Length - FillIdx);
                    FillIdx = FinalHash.Length;
                }
                else
                {
                    Array.Copy(ComputedHash, 0, FinalHash, FillIdx, ComputedHash.Length);
                    FillIdx += ComputedHash.Length;
                }
            }
            while (FillIdx < FinalHash.Length);

            return FinalHash;


        }
    }
}
