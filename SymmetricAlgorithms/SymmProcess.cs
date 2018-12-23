using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace OpenPGPExplorer
{
    public abstract class SymmProcess
    {
        //protected byte[] IterationContext;
        //protected int ByteCount;
        protected SymmProcess SymAlgoProcess;

        public static ICryptoTransform GetDecryptor(S2K S2k, byte[] SessionKey)
        {

            return GetDecryptor(S2k.SymAlgo, S2k.IV, SessionKey);

            //if (S2k.SymAlgo != SymmetricAlgorithmTypes.AES128)
            //    throw new NotImplementedException("Non AES128 not yet implemented");


            //int KeySize = SymmetricAlgorithmTypes.GetKeySize(S2k.SymAlgo);
            //int BlockSizeBytes = S2k.IV.Length;

            //RijndaelManaged aes = new RijndaelManaged
            //{
            //    KeySize = KeySize,
            //    BlockSize = BlockSizeBytes * 8,
            //    FeedbackSize = BlockSizeBytes * 8,
            //    Key = SessionKey,
            //    IV = S2k.IV,
            //    Mode = CipherMode.CFB,
            //    Padding = PaddingMode.None
            //};

            //return aes.CreateDecryptor();
        }

        public static bool IsValidAlgoCode(byte SymAlgo)
        {
            if (SymAlgo == SymmetricAlgorithmTypes.AES128 ||
                SymAlgo == SymmetricAlgorithmTypes.AES192 ||
                SymAlgo == SymmetricAlgorithmTypes.AES256)
                return true;
            return false;
        }

        public static ICryptoTransform GetDecryptor(byte SymAlgo, byte[] SessionKey)
        {           
            return GetDecryptor(SymAlgo, null, SessionKey);
        }

        public static ICryptoTransform GetDecryptor(byte SymAlgo, byte[] IV,  byte[] SessionKey)
        {
            if (!IsValidAlgoCode(SymAlgo))
                throw new NotImplementedException("Algorithm not implemented");


            int KeySize = SymmetricAlgorithmTypes.GetKeySize(SymAlgo);
            int BlockSizeBits = SymmetricAlgorithmTypes.GetBlockSize(SymAlgo);     

            if (IV == null)
                IV = new byte[BlockSizeBits / 8]; // Set to all zeros by default

            RijndaelManaged aes = new RijndaelManaged
            {
                KeySize = KeySize,
                BlockSize = BlockSizeBits,
                FeedbackSize = BlockSizeBits,
                Key = SessionKey,
                IV = IV,
                Mode = CipherMode.CFB,
                Padding = PaddingMode.None
            };

            return aes.CreateDecryptor();
        }

    }
}
