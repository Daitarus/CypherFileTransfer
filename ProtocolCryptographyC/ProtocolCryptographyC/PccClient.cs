using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace ProtocolCryptographyC
{
    public sealed class PccClient
    {
        private IPEndPoint serverEndPoint;
        private Socket socket;
        private byte[] hash;
        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        private Aes aes;
        private FileWork fileWork;

        public PccClient(IPEndPoint serverEndPoint, string login, string password)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.serverEndPoint = serverEndPoint;
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(login + password));
            }
            aes = Aes.Create();
        }
        public bool Connect()
        {
            try
            {
                //tcp connect
                socket.Connect(serverEndPoint);

                //ask get publicKeyRSA
                byte[]? buffer = Segment.PackSegment(TypeSegment.ASK_GET_PKEY, (byte)0, null);
                if(buffer != null)
                {
                    socket.Send(buffer);
                }
                else
                {
                    return false;
                }

                //waiting answer publicKeyRSA
                Segment? segment;
                do
                {
                    segment = Segment.ParseSegment(socket);
                } while ((segment == null) || (segment.Type != TypeSegment.PKEY) || (segment.Payload == null));
                RSAParameters publicKey = rsa.ExportParameters(false);
                publicKey.Modulus = segment.Payload;
                rsa.ImportParameters(publicKey);

                //send hash + aesKey
                int length = hash.Length + aes.Key.Length + aes.IV.Length;
                buffer = new byte[length];
                for (int i = 0; i < hash.Length; i++) 
                {
                    buffer[i] = hash[i];
                }
                for (int i = 0; i < aes.Key.Length; i++) 
                {
                    buffer[hash.Length + i] = aes.Key[i];
                }
                for (int i = 0; i < aes.IV.Length; i++)
                {
                    buffer[hash.Length + aes.Key.Length + i] = aes.IV[i];
                }

                buffer = rsa.Encrypt(buffer, false);
                buffer = Segment.PackSegment(TypeSegment.AUTHORIZATION, (byte)0, buffer);
                if (buffer != null)
                {
                    socket.Send(buffer);
                }
                else
                {
                    return false;
                }
                fileWork = new FileWork(socket);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool TransferFile()
        {
            return fileWork.TransferFile();
        }
        public bool GetFile(string homePath, FileInfo fileInfo)
        {
            return fileWork.GetFile(homePath, fileInfo);
        }
        public void Disconnect()
        {
            try
            {
                if (socket != null)
                {
                    Console.WriteLine("Disconnection!");
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            }
            catch { }
        }
    }
}