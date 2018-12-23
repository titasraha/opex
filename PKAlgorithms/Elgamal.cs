using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPGPExplorer
{
    public class Elgamal : PKAlgorithm
    {
        // Public
        public byte[] P { get; set; }
        public byte[] G { get; set; }
        public byte[] Y { get; set; }

        // Private
        public byte[] X { get; private set; }
        

        public class PublicKeyTransformedElgamalData : ITransformedData
        {
            // Encrypted Value
            public byte[] G_K_ModP;
            public byte[] M_Y_K_ModP;
        }

        // Signature Value
        // ???


        public override ITransformedData LoadPublicKeyTransformedData(TreeBuilder tree)
        {
            var Data = new PublicKeyTransformedElgamalData();

            tree.AddNode("Elgamal Encrypted Data");
            tree.PushByteBlock();                      
            Data.G_K_ModP = tree.ReadMPIBytes("g ^ k Mod p");
            Data.M_Y_K_ModP = tree.ReadMPIBytes("m * y ^ k Mod p");
            tree.PopByteBlock();
            return Data;
        }

        public override ITransformedData LoadSecretKeyTransformedData(TreeBuilder tree)
        {
            throw new NotImplementedException();
        }


        public override ByteBlock GetPrivateByteBlocks()
        {
            ByteBlock Result = new ByteBlock();

            if (PrivateKeyDecrypted)
                Result.AddChildBlock("Elgamal secret exponent x", X.Length.ToString() + " Bytes", 0, X);

            return Result.ChildBlock;
        }


        public override void LoadPublicKey(TreeBuilder tree)
        {

            tree.AddNode("Elgamal Public Key");
            tree.PushByteBlock();
            P = tree.ReadMPIBytes("Elgamal Prime p");
            G = tree.ReadMPIBytes("Elgamal group generator g");
            Y = tree.ReadMPIBytes("Elgamal public key value y");
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

            return false;
        }

        public override byte[] Decrypt(ITransformedData PublicKeyTransformedData)
        {
            throw new NotImplementedException();
        }
    }   
}
