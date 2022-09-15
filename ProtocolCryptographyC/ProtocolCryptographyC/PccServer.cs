using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolCryptographyC
{
    public sealed class PccServer
    {
        public delegate bool Authorization(byte[] hash);
        public delegate void Algorithm(Aes aes);

        private IPEndPoint serverEndPoint;
        private Socket listenSocket  = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        RSACryptoServiceProvider rsa;
        private FileWork fileWork;


        public PccServer(IPEndPoint serverEndPoint, RSACryptoServiceProvider rsa)
        {
            this.serverEndPoint = serverEndPoint;
            this.rsa = rsa;
        }
        public System_Message Start(Authorization authorization, Algorithm algorithm)
        {
            try
			{
				while (true)
				{
					listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					listenSocket.Bind(serverEndPoint);
					listenSocket.Listen(1);
					//get connection
					ClientWork(listenSocket.Accept(), authorization, algorithm);
					listenSocket.Close();
				}
			}
			catch
			{
				return System_Message.ERROR_START;
			}
		}
		private async void ClientWork(Socket socket, Authorization authorization, Algorithm algorithm)
        {
            Segment segment = new Segment();
            byte[] hash = new byte[20];
            Aes aes = Aes.Create();

            try
            {
                //wait first message
                await Task.Run(() => segment = Segment.ParseSegment(socket));
                if (segment != null)
                {
                    if (segment.Type == TypeSegment.ASK_GET_PKEY)
                    {
                        //send publicKeyRSA
                        byte[] publicKeyRSA = rsa.ExportParameters(false).Modulus;
                        byte[]? buffer = Segment.PackSegment(TypeSegment.PKEY, (byte)0, publicKeyRSA);
                        if (buffer != null)
                        {
                            socket.Send(buffer);
                        }

                        //wait RSA(hash+aesKey)
                        await Task.Run(() => segment = Segment.ParseSegment(socket));
                        if (segment != null)
                        {
                            if ((segment.Type == TypeSegment.AUTHORIZATION) && (segment.Payload != null))
                            {
                                //decrypt RSA
                                buffer = rsa.Decrypt(segment.Payload, false);
                                byte[] aesKey = new byte[32];
                                byte[] aesIv = new byte[16];
                                for (int i = 0; i < 20; i++) 
                                {
                                    hash[i] = buffer[i];
                                }
                                for(int i = 0; i < 32; i++)
                                {
                                    aesKey[i] = buffer[20 + i];
                                }
                                for (int i = 0; i < 16; i++) 
                                {
                                    aesIv[i] = buffer[52 + i];
                                }
                                aes.Key = aesKey;
                                aes.IV = aesIv;

                                //authorization
                                if (authorization(hash))
                                {
                                    buffer = Segment.PackSegment(TypeSegment.ANSWER_AUTHORIZATION_YES, (byte)0, null);
                                    socket.Send(buffer);
                                    //algorithm execution
                                    fileWork = new FileWork(socket);
                                    algorithm(aes);
                                }
                                else
                                {
                                    buffer = Segment.PackSegment(TypeSegment.ANSWER_AUTHORIZATION_NO, (byte)0, null);
                                    socket.Send(buffer);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                Disconnect(socket);
            }
        }

        public System_Message TransferFile(Aes aes)
        {
            return fileWork.TransferFile(aes);
        }
        public System_Message GetFile(string homePath, FileInfo fileInfo, Aes aes)
        {
            return fileWork.GetFile(homePath, fileInfo, aes);
        }

        private void Disconnect(Socket socket)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                socket.Close();
            }
        }  
    }
}
