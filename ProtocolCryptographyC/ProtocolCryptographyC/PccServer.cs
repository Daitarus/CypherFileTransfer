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
        private IPEndPoint serverEndPoint;
        private Socket listenSocket;
        RSACryptoServiceProvider rsa;
        private byte[] hashServer;


        public PccServer(IPEndPoint serverEndPoint, RSACryptoServiceProvider rsa)
        {
            this.serverEndPoint = serverEndPoint;
            this.rsa = rsa;
        }
        public bool Start(string login, string password)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                hashServer = sha1.ComputeHash(Encoding.UTF8.GetBytes(login + password));
            }
            try
			{
				while (true)
				{
					listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					listenSocket.Bind(serverEndPoint);
					listenSocket.Listen(1);
					//get connection
					ClientWork(listenSocket.Accept());
					listenSocket.Close();
				}
			}
			catch
			{
				return false;
			}
		}
		private async void ClientWork(Socket socket)
        {
            Console.WriteLine("New connection...");
            Segment segment = new Segment();
            byte[] hash = new byte[20];
            Aes aes = Aes.Create();
            FileWork fileWork = new FileWork(socket);

            try
            {
                //wait first message
                await Task.Run(() => segment = Segment.ParseSegment(socket));
                if (segment == null)
                {
                    Disconnect(socket);
                }
                else
                {
                    if (segment.Type != TypeSegment.ASK_GET_PKEY)
                    {
                        Disconnect(socket);
                    }
                    else
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
                        if (segment == null)
                        {
                            Disconnect(socket);
                        }
                        else
                        {
                            if ((segment.Type != TypeSegment.AUTHORIZATION) || (segment.Payload == null))
                            {
                                Disconnect(socket);
                            }
                            else
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
                                if (!Enumerable.SequenceEqual(hash, hashServer))
                                {
                                    Disconnect(socket);
                                }
                                else
                                {
                                    //mod
                                    fileWork.TransferFile(aes);
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                Disconnect(socket);
            }
        }        
        private bool Disconnect(Socket socket)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                return true;
            }
            catch 
            {
                return false;
            }
            finally
            {
                if(socket != null)
                {
                    socket.Close();
                }
            }
        }
    }
}
