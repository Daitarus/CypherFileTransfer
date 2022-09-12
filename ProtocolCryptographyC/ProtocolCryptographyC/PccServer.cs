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
    public class PccServer
    {
        private IPEndPoint serverEndPoint;
        private Socket listenSocket;
        RSACryptoServiceProvider rsa;

        public PccServer(IPEndPoint serverEndPoint, RSACryptoServiceProvider rsa)
        {
            this.serverEndPoint = serverEndPoint;
            this.rsa = rsa;
        }
        public bool Start()
        {
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
            byte[] hash;
            byte[] data = new byte[256];
            Aes aes;

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
                    if ((segment.Type != TypeSegment.ASK_GET_PKEY) || (segment == null))
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

                        //wait RSA(login+password+aesKey)
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

        private void Disconnect(Socket socket)
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
