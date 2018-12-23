using System;
using System.Linq;
using System.Security.Cryptography;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace OpenPGPExplorer
{
    public class SymEncIPDataPacket : PGPPacket
    {
        public byte Version { private set; get; }

        private ByteBlock ThisBlock { get; set; }


        public override void Parse(TreeBuilder tree)
        {
            Version = tree.ReadByte("Version");

            var block = tree.SkipBytes("Encrypted Data");                                            
            block.ProcessBlock += ExtractData;
            ThisBlock = block;

        }

        private void OnIndexUpdate(int Ctr)
        {
            string Msg = "Building Index " + Ctr.ToString();
            ThisBlock.StatusUpdate?.Invoke(Msg);
        }

        private void StatusUpdate(string Msg, int Percent)
        {
            ThisBlock.StatusUpdate?.Invoke(Msg, Percent);
        }

        private void ExtractData()
        {
            string DecryptedFileName = null;
            string Password = null;

            if (MessageBox.Show("Decrypt Encrypted Data", "Please Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
                return;

            PGPPacket Packet = this;
            List<byte[]> SessionKeys = new List<byte[]>();
            List<PKEncSessionKeyPacket> PKSessions = new List<PKEncSessionKeyPacket>();
            List<SymEncSessionKeyPacket> SymSessions = new List<SymEncSessionKeyPacket>();

            //byte SymmetricAlgorithm = 0;
            string ErrorMsg = "";

            while ((Packet = OpenPGP.GetPreviousPacket(Packet)) != null)
            {
                if (Packet is PKEncSessionKeyPacket PKSessionPacket)
                    PKSessions.Add(PKSessionPacket);
                else if (Packet is SymEncSessionKeyPacket SymKeySessionPacket)
                    SymSessions.Add(SymKeySessionPacket);
            }

            // First try to see if we have any PK Secret keys
            foreach(var PKSession in PKSessions)
            {
                // Get all matching secret keys
                var SecKeyList = OpenPGP.FindPrimaryKey(PKSession.KeyId, true).Where(p => p.PacketTag == 5 || p.PacketTag == 7).ToList();

                foreach (SecretKeyPacket SecKey in SecKeyList)
                {
                    var PKAlgorithm = SecKey.PublicKeyAlgorithm;

                    if (!PKAlgorithm.PrivateKeyDecrypted)
                    {
                        ErrorMsg = "Secret Key is encrypted, click on the encrypted node to decrypt it first";
                        continue;
                    }


                    var ClearBytes = PKAlgorithm.Decrypt(PKSession.PublicKeyTransformedData);

                    long CheckSum = 0;
                    for (int i = 1; i < ClearBytes.Length - 2; i++)
                        CheckSum += ClearBytes[i];

                    CheckSum %= 65536;

                    if ((CheckSum >> 8) != ClearBytes[ClearBytes.Length - 2] || (CheckSum & 0xFF) != ClearBytes[ClearBytes.Length - 1])
                        continue;

                    //SymmetricAlgorithm = ClearBytes[0];
                    SessionKeys.Add(ClearBytes.SubArray(0, ClearBytes.Length - 2));

                }
            }

            if (SessionKeys.Count() == 0)
                if (ErrorMsg != "")
                    throw new Exception(ErrorMsg);

            // If no Public Key Encrypted keys found, then try any Sym keys
            if (SessionKeys.Count() == 0)
                foreach (var SymSession in SymSessions)
                {

                    if (Password == null)
                    {
                        // Ask for password only once
                        PinEntry frm = new PinEntry();

                        if (frm.ShowDialog() == DialogResult.Cancel)
                            break;

                        Password = frm.Password;
                    }

                    byte[] DecryptionKey = SymSession.S2K.GetKey(Password);

                    if (SymSession.EncryptedSessionKey == null)
                    {
                        byte[] SessionKey = new byte[DecryptionKey.Length + 1];
                        SessionKey[0] = SymSession.S2K.SymAlgo;
                        Array.Copy(DecryptionKey, 0, SessionKey, 1, DecryptionKey.Length);
                        SessionKeys.Add(SessionKey);
                    }
                    else
                    {
                        var KeyDecryptor = SymmProcess.GetDecryptor(SymSession.S2K.SymAlgo, DecryptionKey);

                        var BlockSizeBytes = KeyDecryptor.InputBlockSize;
                        int EncryptedLength = SymSession.EncryptedSessionKey.Length;
                        int BlockFillLength = BlockSizeBytes * ((EncryptedLength / BlockSizeBytes) + 1);

                        byte[] CryptBytes = new byte[BlockFillLength];
                        Array.Copy(SymSession.EncryptedSessionKey, 0, CryptBytes, 0, EncryptedLength);


                        byte[] ClearBytes;
                        using (MemoryStream msDecrypt = new MemoryStream())
                        {
                            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, KeyDecryptor, CryptoStreamMode.Write))
                                csDecrypt.Write(CryptBytes, 0, CryptBytes.Length);

                            ClearBytes = msDecrypt.ToArray().SubArray(0, EncryptedLength);
                        }

                        SessionKeys.Add(ClearBytes);

                    }


                }

            //if (Packet == null || (Packet.PacketTag != 1 && Packet.PacketTag != 3))
            //    throw new Exception("No Public-Key/Symmetric-Key encrypted session key packet");

            //if (SecKeyList.Count() == 0)
            //    throw new Exception("Secret Key not available, please add the secret key to decrypt data");


            if (SessionKeys.Count() == 0)                
               throw new Exception("Unable to Decrypt");

            
            // Session Key found                
            DecryptedFileName = TempFiles.GetNewTempFile();


            bool Success = false;

            using (PGPReader fsSource = new PGPReader(FileName))
            {
                fsSource.DoIndexUpdate += OnIndexUpdate;                    
                fsSource.GetPacket(PacketIndex);
                fsSource.DoIndexUpdate -= OnIndexUpdate;
                               
                byte version = fsSource.ReadByte();
                long FileSize = fsSource.BytesRemaining;
                
                fsSource.BookMark();
                foreach (var SessionKey in SessionKeys)
                {
                    try
                    {
                        var decryptor = SymmProcess.GetDecryptor(SessionKey[0], SessionKey.SubArray(1, SessionKey.Length - 1));
                        int BlockSize = decryptor.InputBlockSize;

                        fsSource.GoToBookMark();
                        using (FileStream fsDecrypt = new FileStream(DecryptedFileName, FileMode.Create, FileAccess.Write))
                        {

                            SHA1 Sha1 = new SHA1Managed();
                            byte[] CryptBytes = new byte[BlockSize * 256];  // IMPORTANT: Make sure it is a multiple of block size
                            byte[] PlainBytes = new byte[BlockSize * 256];
                            var BytesRemaining = FileSize - 22;             // 2 byte MDC tag (0xD3, 0x14) and length, 20 byte SHA-1 hash

                            int BlockAlign = (int)(BytesRemaining % BlockSize);
                            BytesRemaining -= BlockAlign;                   // Transformation has to be done in blocks, so make sure it fits in blocks

                            bool FirstTime = true;


                            while (BytesRemaining > 0)
                            {
                                int ReadBytes = (int)BytesRemaining;
                                if (BytesRemaining > CryptBytes.Length)
                                    ReadBytes = CryptBytes.Length;

                                fsSource.Read(CryptBytes, 0, ReadBytes);


                                decryptor.TransformBlock(CryptBytes, 0, ReadBytes, PlainBytes, 0);
                                if (FirstTime)
                                {
                                    // Validate the repeating bytes
                                    if (PlainBytes[BlockSize - 2] != PlainBytes[BlockSize] || PlainBytes[BlockSize - 1] != PlainBytes[BlockSize + 1])
                                        throw new Exception("Pre validation failed, session key may be invalid");
                                    fsDecrypt.Write(PlainBytes, BlockSize + 2, ReadBytes - BlockSize - 2);
                                    FirstTime = false;
                                }
                                else
                                    fsDecrypt.Write(PlainBytes, 0, ReadBytes);

                                Sha1.TransformBlock(PlainBytes, 0, ReadBytes, PlainBytes, 0);

                                StatusUpdate("Decrypting...", (int)(100 * (FileSize - BytesRemaining) / FileSize));
                                BytesRemaining -= ReadBytes;
                            }



                            int FinalBytes = BlockAlign + 22;

                            fsSource.Read(CryptBytes, 0, FinalBytes);
                            decryptor.TransformBlock(CryptBytes, 0, FinalBytes + Program.GetBlockAlignRemainder(FinalBytes, BlockSize), PlainBytes, 0);

                            fsDecrypt.Write(PlainBytes, 0, BlockAlign);


                            // Validate the MDC Packet
                            if (PlainBytes[BlockAlign] != 0xD3 || PlainBytes[BlockAlign + 1] != 0x14)
                                throw new Exception("Modification Detection validation failed, data is corrupt");

                            Sha1.TransformFinalBlock(PlainBytes, 0, BlockAlign + 2); // 0xD3, 0x14 bytes

                            if (!PlainBytes.SubArray(BlockAlign + 2, 20).SequenceEqual(Sha1.Hash))
                                throw new Exception("MDC hash verification failed, Data may have been modified or is corrupt");

                            Success = true;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            if (Success)
            {
                ByteBlock StartBlock = OpenPGP.Process(DecryptedFileName);

                OpenPGP.Validate();
                ThisBlock.AddChildBlock(StartBlock);
            }

            
        }
    }
}
