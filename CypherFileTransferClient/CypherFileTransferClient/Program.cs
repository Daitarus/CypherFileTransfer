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
                PrintMessage.PrintSM("Please, enter server's ip: ", ConsoleColor.White, false);
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
            errorEnter = false;

            //enter authorizationString
            PrintMessage.PrintSM("Please, enter password for connect: ", ConsoleColor.White, false);
            string? authorizationString = Console.ReadLine();

            //start client
            IPEndPoint ipPoint = new IPEndPoint(ip, port);
            PccClient pccClient = new PccClient(ipPoint, authorizationString);
            System_Message system_message;
            system_message = pccClient.Connect();
            if (system_message == System_Message.CONNECTED)
            {
                PrintMessage.PrintSM("Connect !", ConsoleColor.Yellow, true);
                //get file
                string? fileName;
                do
                {
                    //enter fileName
                    do
                    {
                        PrintMessage.PrintSM("Please, enter file name: ", ConsoleColor.White, false);
                        fileName = Console.ReadLine();
                        if((fileName == null) || (fileName == ""))
                        {
                            PrintMessage.PrintSM("Error: empty file name !!!", ConsoleColor.Red, true);
                        }
                    } while ((fileName == null) || (fileName == ""));
                    system_message = pccClient.GetFile("", new FileInfo(fileName));
                    if(system_message == System_Message.FILE_WAS_TRANSFER)
                    {
                        PrintMessage.PrintSM("File was get !", ConsoleColor.Cyan, true);
                    }
                    if(system_message == System_Message.NOT_FOUND_ALLOWABLE_FILE)
                    {
                        PrintMessage.PrintSM("File not faund or very big !", ConsoleColor.Red, true);
                    }
                } while ((system_message == System_Message.FILE_WAS_TRANSFER) || (system_message == System_Message.NOT_FOUND_ALLOWABLE_FILE));
                PrintMessage.PrintSM("Error: File was not tranfer !!!", ConsoleColor.Red, true);
            }
            else
            {
                PrintMessage.PrintSM("Error: No connect !!!", ConsoleColor.Red, true);
            }
            Console.ReadLine();
        }
    }
}