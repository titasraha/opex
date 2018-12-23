using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace OpenPGPExplorer
{
    public abstract class PKAlgorithm
    {
        public S2K S2K { protected get; set; }

        public byte[] EncryptedPrivateKey { get; set; }
        public bool PrivateKeyDecrypted { get; protected set; }

        public abstract bool SetPrivate(byte[] ClearBytes);

        public abstract ByteBlock GetPrivateByteBlocks();

        public abstract void LoadPublicKey(TreeBuilder tree);


        public abstract ITransformedData LoadPublicKeyTransformedData(TreeBuilder tree);
        public abstract ITransformedData LoadSecretKeyTransformedData(TreeBuilder tree);

        public abstract byte[] Decrypt(ITransformedData PublicKeyTransformedData);

        public virtual bool VerifySignature(SignaturePacket SigPacket)
        {
            if (SigPacket.SecretKeyTransformedData == null)
                return false;

            if (SigPacket.HashedData == null)
                return false;

            return true;
        }

        public PKAlgorithm()
        {
            PrivateKeyDecrypted = false;
        }

        public static PKAlgorithm CreatePKAlgorithm(byte AlgoCode)
        {
            if (AlgoCode == PKAlgorithmTypes.RSA || 
                AlgoCode == PKAlgorithmTypes.RSA_ENCRYPT || 
                AlgoCode == PKAlgorithmTypes.RSA_SIGN)
                return new RSA();
            if (AlgoCode == PKAlgorithmTypes.DSA)
                return new DSA();
            if (AlgoCode == PKAlgorithmTypes.Elgamal)
                return new Elgamal();
            return null;
        }

        public bool DecryptS2K(string Password)
        {
            byte[] SessionKey = S2K.GetKey(Password);

            var decryptor = SymmProcess.GetDecryptor(S2K, SessionKey);

            var BlockSizeBytes = S2K.IV.Length;
            int EncryptedLength = EncryptedPrivateKey.Length;
            int BlockFillLength = BlockSizeBytes * ((EncryptedLength / BlockSizeBytes) + 1);

            byte[] CryptBytes = new byte[BlockFillLength];
            Array.Copy(EncryptedPrivateKey, 0, CryptBytes, 0, EncryptedLength);


            byte[] ClearBytes;
            using (MemoryStream msDecrypt = new MemoryStream())
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                {
                    csDecrypt.Write(CryptBytes, 0, CryptBytes.Length);
                    csDecrypt.Close();
                }
                ClearBytes = msDecrypt.ToArray().SubArray(0, EncryptedLength);
            }

            // Validate Decrypted Key
            int CheckLen = (S2K.S2KUsage == 254) ? 20 : 2;
            int DataLen = ClearBytes.Length - CheckLen;
            byte[] CheckSum = ClearBytes.SubArray(DataLen, CheckLen);
            bool bSuccess;

            if (CheckLen == 2)
            {
                long C = 0;
                for (int i = 0; i < DataLen; i++)
                    C += ClearBytes[i];
                C = C % 65536;

                bSuccess = ((C >> 8) == CheckSum[0] && (C & 0xff) == CheckSum[1]);

            }
            else
            {
                SHA1Managed sha = new SHA1Managed();
                byte[] HashToCompare = sha.ComputeHash(ClearBytes.SubArray(0, DataLen));
                bSuccess = HashToCompare.SequenceEqual(CheckSum);
            }

            if (bSuccess)
                return SetPrivate(ClearBytes);
            
                
            return false;
        }
    }

    public interface ITransformedData
    {

    }
}
