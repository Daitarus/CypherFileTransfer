using System.Net;
using ProtocolCryptographyC;

namespace CypherFileTransferClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool errorEnter = false;
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            int port = 5000;

            //enter ip
            while (!errorEnter)
            {
                PrintMessage.PrintColorMessage("Please, enter server's ip: ", ConsoleColor.White, false);
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
            errorEnter = false;

            //enter authorizationString
            PrintMessage.PrintColorMessage("Please, enter password for connect: ", ConsoleColor.White, false);
            string authorizationString = Console.ReadLine();

            //connect client
            IPEndPoint ipPoint = new IPEndPoint(ip, port);
            PccClient pccClient = new PccClient(ipPoint, authorizationString);
            PccSystemMessage systemMessage;
            systemMessage = pccClient.Connect();

            //get files
            if (systemMessage.Key == PccSystemMessageKey.INFO)
            {
                PrintMessage.PrintColorMessage(systemMessage.Message, ConsoleColor.Cyan, true);

                string fileName;
                string path;

                do
                {
                    //enter local path
                    PrintMessage.PrintColorMessage("Please, enter local path: ", ConsoleColor.White, false);
                    path = Console.ReadLine();
                    if (path != null && path[path.Length - 1] != '\\') 
                    {
                        path += '\\';
                    }

                    //enter fileName
                    do
                    {
                        PrintMessage.PrintColorMessage("Please, enter file name: ", ConsoleColor.White, false);
                        fileName = Console.ReadLine();
                        if((fileName == null) || (fileName == ""))
                        {
                            PrintMessage.PrintColorMessage("Error: empty file name !!!", ConsoleColor.Red, true);
                        }
                    } while ((fileName == null) || (fileName == ""));
                    systemMessage = pccClient.fileTransport.SendFileInfo(fileName);
                    PrintMessage.PrintSystemMessage(systemMessage);

                    systemMessage = pccClient.fileTransport.GetFile(path);
                    PrintMessage.PrintSystemMessage(systemMessage);

                } while ((systemMessage.Key == PccSystemMessageKey.INFO) || (systemMessage.Key == PccSystemMessageKey.WARRNING));

                //disconnect
                PrintMessage.PrintColorMessage(pccClient.Disconnect().Message, ConsoleColor.Yellow, true);
            }
            else
            {
                PrintMessage.PrintColorMessage(systemMessage.Message, ConsoleColor.Red, true);
            }
            Console.ReadLine();
        }
    }
}