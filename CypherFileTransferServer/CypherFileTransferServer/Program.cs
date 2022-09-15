using ProtocolCryptographyC;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace CypherFileTransferServer
{
    internal class Program
    {
        static byte[] hashServer;
        static PccServer pccServer;

        static void Main(string[] args)
        {
			bool errorEnter = false;
			IPAddress? ip = IPAddress.Parse("127.0.0.1");
			int port = 5000;

            //enter ip
            while (!errorEnter)
            {
                PrintMessage.PrintSM("Please, enter your ip: ", ConsoleColor.White, false);
                errorEnter = IPAddress.TryParse(Console.ReadLine(), out ip);
                if (!errorEnter)
                {
                    PrintMessage.PrintSM("Error: Incorrect data !!!", ConsoleColor.Red, true);
                }
            }
            errorEnter = false;

            //enter port
            while (!errorEnter)
            {
                PrintMessage.PrintSM("Please, enter tcp port: ", ConsoleColor.White, false);
                errorEnter = int.TryParse(Console.ReadLine(), out port);
                if (!errorEnter)
                {
                    PrintMessage.PrintSM("Error: Incorrect data !!!", ConsoleColor.Red, true);
                }
            }

            //enter authorizationString
            PrintMessage.PrintSM("Please, password for connect: ", ConsoleColor.White, false);
            hashServer = GetHash(Console.ReadLine());


            //start server
            PrintMessage.PrintSM("Server Start...", ConsoleColor.Yellow, true);
			IPEndPoint serverEndPoint = new IPEndPoint(ip, port);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            pccServer = new PccServer(serverEndPoint, rsa);
			pccServer.Start(Authorization, Algorithm);
		}

        private static byte[] GetHash(string authoriazationString)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
               return sha1.ComputeHash(Encoding.UTF8.GetBytes(authoriazationString));
            }
        }
        private static bool Authorization(byte[] hash)
        {
            if (Enumerable.SequenceEqual(hash, hashServer))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void Algorithm(Aes aes)
        {
            System_Message system_message;
            do
            {
                system_message = pccServer.TransferFile(aes);
                if(system_message == System_Message.FILE_WAS_TRANSFER)
                {
                    PrintMessage.PrintSM("File was tranfer !", ConsoleColor.Cyan, true);
                }
                if(system_message == System_Message.NOT_FOUND_ALLOWABLE_FILE)
                {
                    PrintMessage.PrintSM("File not faund or very big !", ConsoleColor.Cyan, true);
                }
            } while ((system_message == System_Message.FILE_WAS_TRANSFER) || (system_message == System_Message.NOT_FOUND_ALLOWABLE_FILE));
        }
	}
}