using NLog;
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
        static Logger logger = LogManager.GetCurrentClassLogger();

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
            PrintMessage.PrintSM("Please, enter password for connect: ", ConsoleColor.White, false);
            hashServer = GetHash(Console.ReadLine());


            //start server
            PrintMessage.PrintSM("Server Start...", ConsoleColor.Yellow, true);
			IPEndPoint serverEndPoint = new IPEndPoint(ip, port);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            pccServer = new PccServer(serverEndPoint, rsa);
			pccServer.Start(Authorization, Algorithm, PrintSystemMessage);
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

        private static void Algorithm(ClientInfo clientInfo)
        {
            string system_message, logString;

            do
            {
                system_message = pccServer.TransferFile(clientInfo.aes);
                logString = $"{clientInfo.Ip}:{clientInfo.Port} - {system_message}";
                if (system_message[0]=='E')
                {
                    logger.Error(logString);
                }
                if(system_message[0]=='W')
                {
                    logger.Warn(logString);
                }
                if (system_message[0]=='F')
                {
                    logger.Fatal(logString);
                }
                if (system_message[0]=='I')
                {
                    logger.Info(logString);
                }
            } while ((system_message[0]=='W') || (system_message[0]=='I'));
        }

        private static void PrintSystemMessage(string SystemMessage)
        {
            if (SystemMessage[0] == 'E')
            {
                logger.Error(SystemMessage);
            }
            if (SystemMessage[0] == 'W')
            {
                logger.Warn(SystemMessage);
            }
            if (SystemMessage[0] == 'F')
            {
                logger.Fatal(SystemMessage);
            }
            if (SystemMessage[0] == 'I')
            {
                logger.Info(SystemMessage);
            }
        }
	}
}