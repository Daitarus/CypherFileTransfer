using ProtocolCryptographyC;
using System.Net;
using System.Security.Cryptography;

namespace CypherFileTransferServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
			bool errorEnter = false;
			IPAddress? ip = IPAddress.Parse("127.0.0.1");
			int port = 5000;

            //enter ip

            //while (!errorEnter)
            //{
            //    PrintMessage.PrintSM("Please, enter your ip: ", ConsoleColor.White, false);
            //    errorEnter = IPAddress.TryParse(Console.ReadLine(), out ip);
            //    if (!errorEnter)
            //    {
            //        PrintMessage.PrintSM("Error: Incorrect data !!!", ConsoleColor.Red, true);
            //    }
            //}
            //errorEnter = false;

            //enter port

            //while (!errorEnter)
            //{
            //    PrintMessage.PrintSM("Please, enter tcp port: ", ConsoleColor.White, false);
            //    errorEnter = int.TryParse(Console.ReadLine(), out port);
            //    if (!errorEnter)
            //    {
            //        PrintMessage.PrintSM("Error: Incorrect data !!!", ConsoleColor.Red, true);
            //    }
            //}

			//start server
			PrintMessage.PrintSM("Server Start", ConsoleColor.Yellow, true);
			IPEndPoint serverEndPoint = new IPEndPoint(ip, port);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            PccServer pccServer = new PccServer(serverEndPoint, rsa);
			pccServer.Start("admin", "pa$$w0rd");
		}
	}
}