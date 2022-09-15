using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolCryptographyC
{
    internal class FileWork
    {
        public Socket socket;
        
        public FileWork(Socket socket)
        {
            this.socket = socket;
        }
        public System_Message TransferFile(Aes aes)
        {
            try
            {
                //wait ask get file + decrypt fileInfo
                Segment? segment = Segment.ParseSegment(socket);
                if(segment == null)
                {
                    return System_Message.ERROR_ASK_GET_FILE;
                }
                if((segment.Type != TypeSegment.ASK_GET_FILE) || (segment.Payload == null))
                {
                    return System_Message.ERROR_ASK_GET_FILE;
                }
                segment.DecryptPayload(aes);
                FileInfo fileInfo = new FileInfo(Encoding.UTF8.GetString(segment.Payload));

                byte[]? bufferFile = null;
                byte[]? buffer = null;

                //check file and send aes(system message)
                if (!fileInfo.Exists)
                {
                    buffer = Segment.PackSegment(TypeSegment.FILE, (byte)0, EncryptAES(Encoding.UTF8.GetBytes("error"),aes));
                    socket.Send(buffer);
                    return System_Message.NOT_FOUND_ALLOWABLE_FILE;
                }
                long numAllBlock = (long)Math.Ceiling((double)fileInfo.Length / (double)Segment.lengthBlockFile);
                if ((fileInfo.Length == 0) || (numAllBlock >= 256))
                {
                    buffer = Segment.PackSegment(TypeSegment.FILE, (byte)0, EncryptAES(Encoding.UTF8.GetBytes("error"), aes));
                    socket.Send(buffer);
                    return System_Message.NOT_FOUND_ALLOWABLE_FILE;
                }
                //send first file part aes(system message or number of block + fileInfo)
                buffer = Encoding.UTF8.GetBytes(fileInfo.Name);
                bufferFile = new byte[buffer.Length + 1];
                bufferFile[0] = (byte)numAllBlock;
                for (int i = 0; i < buffer.Length; i++)
                {
                    bufferFile[i + 1] = buffer[i];
                }
                buffer = Segment.PackSegment(TypeSegment.FILE, (byte)0, EncryptAES(bufferFile, aes));
                if (buffer == null)
                {
                    return System_Message.NOT_FILE_INFO;
                }
                socket.Send(buffer);

                //send file
                using (FileStream fstream = File.Open(fileInfo.Name, FileMode.Open))
                {
                    //load file part
                    int numReadByte;
                    for (int i = 0; i < numAllBlock; i++)
                    {
                        buffer = new byte[Segment.lengthBlockFile];
                        fstream.Seek(i * Segment.lengthBlockFile, SeekOrigin.Begin);
                        numReadByte = fstream.Read(buffer);
                        bufferFile = new byte[numReadByte];
                        for (int j = 0; j < numReadByte; j++)
                        {
                            bufferFile[j] = buffer[j];
                        }

                        //send part file
                        buffer = Segment.PackSegment(TypeSegment.FILE, (byte)i, EncryptAES(bufferFile, aes));
                        if (buffer == null)
                        {
                            return System_Message.FILE_WAS_NOT_TRANSFER;
                        }
                        socket.Send(buffer);
                    }
                }
                return System_Message.FILE_WAS_TRANSFER;
            }
            catch
            {
                return System_Message.FILE_WAS_NOT_TRANSFER;
            }
        }
        public System_Message GetFile(string homePath, FileInfo fileInfo, Aes aes)
        {
            try
            {
                //encrypt fileInfo + ask get file
                byte[]? bufferFile = Segment.PackSegment(TypeSegment.ASK_GET_FILE, (byte)0, EncryptAES(Encoding.UTF8.GetBytes(fileInfo.Name),aes));
                if (bufferFile == null)
                {
                    return System_Message.ERROR_ASK_GET_FILE;
                }
                socket.Send(bufferFile);

                //get first part file aes(system message or number of block + fileInfo)
                Segment? segment;
                segment = Segment.ParseSegment(socket);
                
                if (segment == null)
                {
                    return System_Message.NOT_FILE_INFO;
                }
                if ((segment.Type != TypeSegment.FILE) || (segment.Payload == null))
                {
                    return System_Message.NOT_FILE_INFO;
                }
                segment.DecryptPayload(aes);
                if (Encoding.UTF8.GetString(segment.Payload) == "error")
                {
                    return System_Message.NOT_FOUND_ALLOWABLE_FILE;
                }

                byte numAllBlock = segment.Payload[0];
                byte[] buffer = new byte[segment.Payload.Length - 1];
                for (int i = 1; i < segment.Payload.Length; i++)
                {
                    buffer[i - 1] = segment.Payload[i];
                }
                fileInfo = new FileInfo(homePath + Encoding.UTF8.GetString(buffer));

                //get file
                using (FileStream fstream = new FileStream(homePath + fileInfo, FileMode.OpenOrCreate))
                {
                    for (int i = 0; i < numAllBlock; i++)
                    {
                        segment = Segment.ParseSegment(socket);
                        if (segment == null)
                        {
                            return System_Message.FILE_WAS_NOT_TRANSFER;
                        }
                        if ((segment.Type != TypeSegment.FILE) || (segment.Payload == null))
                        {
                            return System_Message.FILE_WAS_NOT_TRANSFER;
                        }
                        segment.DecryptPayload(aes);
                        fstream.Seek(i * Segment.lengthBlockFile, SeekOrigin.Begin);
                        fstream.Write(segment.Payload);
                    }
                }
                return System_Message.FILE_WAS_TRANSFER;
            }
            catch
            {
                return System_Message.FILE_WAS_NOT_TRANSFER;
            }
        }

        private static byte[] EncryptAES(byte[] data, Aes aes)
        {
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (var ms = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }

        public static byte[] DecryptAES(byte[] data, Aes aes)
        {
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using (var ms = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }
    }
}
