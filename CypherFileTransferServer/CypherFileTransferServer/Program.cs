using ProtocolCryptographyC;
using System.Net;
using System.Text;
using CryptL;

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
                PrintMessage.PrintColorMessage("Please, enter your ip: ", ConsoleColor.White, false);
                errorEnter = IPAddress.TryParse(Console.ReadLine(), out ip);
                if (!errorEnter)
                {
                    PrintMessage.PrintColorMessage("Error: Incorrect data !!!", ConsoleColor.Red, true);
                }
            }
            errorEnter = false;

            //enter port
            while (!errorEnter)
            {
                PrintMessage.PrintColorMessage("Please, enter tcp port: ", ConsoleColor.White, false);
                errorEnter = int.TryParse(Console.ReadLine(), out port);
                if (!errorEnter)
                {
                    PrintMessage.PrintColorMessage("Error: Incorrect data !!!", ConsoleColor.Red, true);
                }
            }

            //enter authorizationString
            PrintMessage.PrintColorMessage("Please, enter password for connect: ", ConsoleColor.White, false);
            hashServer = HashSHA256.GetHash(Encoding.UTF8.GetBytes(Console.ReadLine()));


            //start server
            PrintMessage.PrintColorMessage("Server Start...", ConsoleColor.Yellow, true);
			IPEndPoint serverEndPoint = new IPEndPoint(ip, port);
            CryptRSA cryptRSA = new CryptRSA();
            pccServer = new PccServer(serverEndPoint, cryptRSA);
			pccServer.Start(Authorization, MainServerAlgorithm, PrintMessage.PrintSystemMessage);
		}

        private static bool Authorization(byte[] hash)
        {
            return Enumerable.SequenceEqual(hash, hashServer);
        }

        private static void MainServerAlgorithm(ClientInfo clientInfo)
        {
            string fileInfo, additionalMessage = $"{clientInfo.ClientEndPoint.Address}:{clientInfo.ClientEndPoint.Port}";
            PccSystemMessage systemMessage;

            do
            {
                systemMessage = pccServer.fileTransport.GetFileInfo(out fileInfo);
                systemMessage.AdditionalMessage = additionalMessage;
                PrintMessage.PrintSystemMessage(systemMessage);
                if (systemMessage.Key == PccSystemMessageKey.INFO)
                {
                    systemMessage = pccServer.fileTransport.SendFile(fileInfo);
                    systemMessage.AdditionalMessage = additionalMessage;
                    PrintMessage.PrintSystemMessage(systemMessage);
                }
            } while ((systemMessage.Key == PccSystemMessageKey.WARRNING) || (systemMessage.Key == PccSystemMessageKey.INFO));
        }        
	}
}