using System;
using System.Security.Cryptography;

namespace OpenPGPExplorer
{
    public class DSA : PKAlgorithm
    {
        // Public
        public byte[] P { get; set; }
        public byte[] Q { get; set; }
        public byte[] G { get; set; }
        public byte[] Y { get; set; }

        // Private
        public byte[] X { get; private set; }

        // Encrypted Value
        // ???
        

        public struct SecretKeyTransformedDSAData : ITransformedData
        {
            // Signature Value
            public byte[] R;
            public byte[] S;
        }

        public override ITransformedData LoadSecretKeyTransformedData(TreeBuilder tree)
        {
            var Data = new SecretKeyTransformedDSAData();

            tree.AddNode("DSA Signed Data");
            tree.PushByteBlock();
            Data.R = tree.ReadMPIBytes("r");
            Data.S = tree.ReadMPIBytes("r");
            tree.PopByteBlock();

            return Data;

        }

        public override ITransformedData LoadPublicKeyTransformedData(TreeBuilder tree)
        {
            throw new NotImplementedException();
        }


        public override ByteBlock GetPrivateByteBlocks()
        {
            ByteBlock Result = new ByteBlock();

            if (PrivateKeyDecrypted)            
                Result.AddChildBlock("DSA secret exponent x", X.Length.ToString() + " Bytes", 0, X);
                
            
            return Result.ChildBlock;
        }

        public override void LoadPublicKey(TreeBuilder tree)
        {

            tree.AddNode("DSA Public Key");
            tree.PushByteBlock();
            P = tree.ReadMPIBytes("DSA Prime p");
            Q = tree.ReadMPIBytes("DSA group order q");
            G = tree.ReadMPIBytes("DSA group generator g");
            Y = tree.ReadMPIBytes("DSA public key value y");
            tree.PopByteBlock();
          
        }


        public override bool SetPrivate(byte[] ClearBytes)
        {
            PrivateKeyDecrypted = false;
            int Idx = 0;

            X = Program.GetMPIBytes(ClearBytes, ref Idx);

            PrivateKeyDecrypted = true;


            return PrivateKeyDecrypted;
        }

        public override bool VerifySignature(SignaturePacket SigPacket)
        {
            if (!base.VerifySignature(SigPacket))
                return false;            

            if (!(SigPacket.SecretKeyTransformedData is SecretKeyTransformedDSAData dsa))
                return false;

            var csp = new DSACryptoServiceProvider();
            DSAParameters param = new DSAParameters
            {
                P = P,
                Q = Q,
                G = G,
                Y = Y
            };
            

            csp.ImportParameters(param);

            if (SigPacket.HashedData.Length < Q.Length)
                return false;

            var SubHash = SigPacket.HashedData.SubArray(0, Q.Length);
            byte[] SigVerify = new byte[dsa.R.Length + dsa.S.Length];

            Array.Copy(dsa.R, 0, SigVerify, 0, dsa.R.Length);
            Array.Copy(dsa.S, 0, SigVerify, dsa.R.Length, dsa.S.Length);


            try
            {
                bool IsMatch = csp.VerifyHash(SubHash, SigPacket.HashAlgorithmName.Name, SigVerify);
                return IsMatch;
            }
            catch { }

            return false;

        }

        public override byte[] Decrypt(ITransformedData PublicKeyTransformedData)
        {
            throw new NotImplementedException();
        }
    }
}
