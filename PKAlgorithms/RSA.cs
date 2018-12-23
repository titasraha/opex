using System;
using System.Security.Cryptography;
using System.Numerics;
using System.Linq;

namespace OpenPGPExplorer
{
    public class RSA:PKAlgorithm
    {
        // Public
        public byte[] ModN { get; set; }
        public byte[] Exp { get; set; }

        // Private
        public byte[] D { get; private set; }
        public byte[] P { get; private set; }
        public byte[] Q { get; private set; }
        public byte[] U { get; private set; }


        public class PublicKeyTransformedRSAData : ITransformedData
        {
            public byte[] M_E_ModN;
        }

        public class SecretKeyTransformedRSAData : ITransformedData
        {
            public byte[] M_D_ModN;
        }


        public override ITransformedData LoadPublicKeyTransformedData(TreeBuilder tree)
        {
            var Data = new PublicKeyTransformedRSAData();
            
            tree.AddNode("RSA Encrypted Data");
            tree.PushByteBlock();
            Data.M_E_ModN = tree.ReadMPIBytes("m ^ e Mod n");
            tree.PopByteBlock();
            return Data;
        }

        public override ITransformedData LoadSecretKeyTransformedData(TreeBuilder tree)
        {
            var Data = new SecretKeyTransformedRSAData();

            tree.AddNode("RSA Signed Data");
            tree.PushByteBlock();
            Data.M_D_ModN = tree.ReadMPIBytes("m ^ d Mod n");
            tree.PopByteBlock();

            return Data;

        }

        public override ByteBlock GetPrivateByteBlocks()
        {
            ByteBlock Result = new ByteBlock();

            if (PrivateKeyDecrypted)
            {
                
                Result.AddChildBlock("RSA secret exponent d", D.Length.ToString() + " Bytes", 0, D);
                Result.AddChildBlock("RSA secret prime value p", P.Length.ToString() + " Bytes", 0, P);
                Result.AddChildBlock("RSA secret prime value q (p < q)", Q.Length.ToString() + " Bytes", 0, Q);
                Result.AddChildBlock("u, the multiplicative inverse of p, mod q", U.Length.ToString() + " Bytes", 0, U);
            }            
            return Result.ChildBlock;
        }


        public override void LoadPublicKey(TreeBuilder tree)
        {
            tree.AddNode("RSA Public Key");
            tree.PushByteBlock();
            ModN = tree.ReadMPIBytes("Mod n");
            Exp = tree.ReadMPIBytes("Exp");
            tree.PopByteBlock();
        }


        public override bool SetPrivate(byte[] ClearBytes)
        {
            PrivateKeyDecrypted = false;
            int Idx = 0;

            D = Program.GetMPIBytes(ClearBytes, ref Idx);
            P = Program.GetMPIBytes(ClearBytes, ref Idx);
            Q = Program.GetMPIBytes(ClearBytes, ref Idx);
            U = Program.GetMPIBytes(ClearBytes, ref Idx);
                  
            PrivateKeyDecrypted = true;


            return PrivateKeyDecrypted;
         
        }

        public override bool VerifySignature(SignaturePacket SigPacket)
        {
            if (!base.VerifySignature(SigPacket))
                return false;

            if (!(SigPacket.SecretKeyTransformedData is SecretKeyTransformedRSAData rsa))
                return false;



            var csp = new RSACryptoServiceProvider(ModN.Length * 8);
            RSAParameters param = new RSAParameters
            {
                Modulus = ModN,
                Exponent = Exp
            };
            byte[] SigVerify = new byte[ModN.Length];

            csp.ImportParameters(param);
            

            if (rsa.M_D_ModN.Length > ModN.Length)
                return false;
            

            if (rsa.M_D_ModN.Length < ModN.Length)
                Array.Copy(rsa.M_D_ModN, 0, SigVerify, ModN.Length - rsa.M_D_ModN.Length, rsa.M_D_ModN.Length);
            else
                Array.Copy(rsa.M_D_ModN, SigVerify, ModN.Length);

            try
            {
                bool IsMatch = csp.VerifyHash(SigPacket.HashedData, SigVerify, SigPacket.HashAlgorithmName, RSASignaturePadding.Pkcs1);
                return IsMatch;
            }
            catch { }

            return false;


        }

        private RSAParameters GetFullPrivateParameters()
        {
          
            BigInteger p = new BigInteger(CopyAndReverse(P));
            BigInteger q = new BigInteger(CopyAndReverse(Q));
            BigInteger e = new BigInteger(CopyAndReverse(Exp));
            BigInteger modulus = new BigInteger(CopyAndReverse(ModN));
          
            var n = p * q;
           
            var phiOfN = n - p - q + 1; 

            var d = ModInverse(e, phiOfN);

            var dp = d % (p - 1);
            var dq = d % (q - 1);
            var qInv = ModInverse(q, p);


            return new RSAParameters
            {
                P = P,
                Q = Q,
                Exponent = Exp,
                Modulus = ModN,
                D = D,
                DP = GetUnsignedBytes(dp.ToByteArray()),
                DQ = GetUnsignedBytes(dq.ToByteArray()),
                InverseQ = GetUnsignedBytes(qInv.ToByteArray()),
            };
        }

        private BigInteger ModInverse(BigInteger a, BigInteger n)
        {
            BigInteger t = 0, nt = 1, r = n, nr = a;

            if (n < 0)
            {
                n = -n;
            }

            if (a < 0)
            {
                a = n - (-a % n);
            }

            while (nr != 0)
            {
                var quot = r / nr;

                var tmp = nt; nt = t - quot * nt; t = tmp;
                tmp = nr; nr = r - quot * nr; r = tmp;
            }

            if (r > 1) throw new ArgumentException(nameof(a) + " is not convertible.");
            if (t < 0) t = t + n;
            return t;
        }

        private byte[] CopyAndReverse(byte[] data)
        {
            int Adjust = 0;

            if ((data[0] & 0x80) != 0)
                Adjust = 1; // If high bit is set, prefix 0 so it is not misinterpreted as a negative number

            byte[] reversed = new byte[data.Length + Adjust]; 

            Array.Copy(data, 0, reversed, Adjust, data.Length);
            Array.Reverse(reversed);
            return reversed;
        }

        public byte[] GetUnsignedBytes(byte[] data)
        {
            int Adjust = 0;
            if (data[data.Length - 1] == 0x00)
                Adjust = 1;

            return data.Reverse().ToArray().SubArray(Adjust, data.Length - Adjust);
        }

        public override byte[] Decrypt(ITransformedData PublicKeyTransformedData)
        {
            if (!(PublicKeyTransformedData is PublicKeyTransformedRSAData RSAData))
                return null;
            
            var csp = new RSACryptoServiceProvider(ModN.Length * 8);

            RSAParameters param = GetFullPrivateParameters();

            csp.ImportParameters(param);

            return csp.Decrypt(RSAData.M_E_ModN, false);


        }
    }
}
