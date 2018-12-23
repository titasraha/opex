using System;
using System.Windows.Forms;

namespace OpenPGPExplorer
{
    public class SecretKeyPacket: PublicKeyPacket
    {
        ByteBlock SecretKeyNode;

        private void ExtractPrivateKey()
        {
            if (!PublicKeyAlgorithm.PrivateKeyDecrypted)
            {
                PinEntry frm = new PinEntry();
                bool Result = false;
                do
                {
                    if (frm.ShowDialog() == DialogResult.Cancel)
                        break;

                    Result = PublicKeyAlgorithm.DecryptS2K(frm.Password);

                } while (!Result);

                if (Result)
                    SecretKeyNode.AddChildBlock(PublicKeyAlgorithm.GetPrivateByteBlocks());
               
            }
        }

        public override void Parse(TreeBuilder tree)
        {
            base.Parse(tree);

            bool IsEncrypted = true;
            tree.SetBookMark();
            var S2K = new S2K
            {
                S2KUsage = tree.ReadByte()
            };

            if (S2K.S2KUsage == 254 || S2K.S2KUsage == 255)
            {
                S2K.SymAlgo = tree.ReadByte("Symmetric Algorithm", SymmetricAlgorithmTypes.Get);

                byte S2KSpecifier = tree.ReadByte("S2K Specifier", S2KTypes.Get);

                if (S2KSpecifier != S2KTypes.Salted && S2KSpecifier != S2KTypes.Simple && S2KSpecifier != S2KTypes.IteratedAndSalted)
                {
                    //tree.AddCalculated("Invalid S2K", S2KSpecifier.ToString());
                    tree.AddCalculated("Unable to Process", S2KSpecifier.ToString(),  ByteBlockType.CalculatedError);
                    return;
                }

                S2K.HashAlgorithm = tree.ReadByte("Hash Algorithm", HashAlgorithmTypes.Get);                

                if (S2KSpecifier == S2KTypes.Salted || S2KSpecifier == S2KTypes.IteratedAndSalted)
                {
                    S2K.Salt = tree.ReadBytes("Salt", 8);
                    if (S2KSpecifier == S2KTypes.IteratedAndSalted)
                    {
                        byte CodedCount = tree.ReadByte("Coded Iteration");                        
                        S2K.ByteCount = (16 + (CodedCount & 15)) << ((CodedCount >> 4) + 6);
                    }
                }

                int BlockSizeBytes = SymmetricAlgorithmTypes.GetBlockSize(S2K.SymAlgo) / 8;
                S2K.IV = tree.ReadBytes("IV", BlockSizeBytes);
            }
            else
            {

                byte SymAlgo = S2K.S2KUsage;
                S2K.SymAlgo = SymAlgo;

                if (SymAlgo != 0)
                {
                    tree.GoToBookMark();
                    tree.ReadByte("Symmetric Algorithm", SymmetricAlgorithmTypes.Get);                    
                    int BlockSize = SymmetricAlgorithmTypes.GetBlockSize(SymAlgo) / 8;
                    S2K.IV = tree.ReadBytes("IV", BlockSize);                    
                }
                else
                {
                    IsEncrypted = false;
                }

            }

            PublicKeyAlgorithm.S2K = S2K;

            if (IsEncrypted)
            {                
                byte[] Encrypted = tree.ReadBytes("Encrypted Secret Key");
                PublicKeyAlgorithm.EncryptedPrivateKey = Encrypted;
                tree.CurrentBlock.ProcessBlock += ExtractPrivateKey;
                SecretKeyNode = tree.CurrentBlock;
            }
            else
            {
                byte[] ClearBytes = tree.ReadBytes("Unencrypted Secret Key");

                var SecBlockClear = PublicKeyAlgorithm.SetPrivate(ClearBytes);
                tree.AddChild(PublicKeyAlgorithm.GetPrivateByteBlocks());
            }
        }        
    }
}
