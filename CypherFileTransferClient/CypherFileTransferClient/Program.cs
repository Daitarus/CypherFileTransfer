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

            //connect client
            IPEndPoint ipPoint = new IPEndPoint(ip, port);
            PccClient pccClient = new PccClient(ipPoint, authorizationString);
            string system_message;
            system_message = pccClient.Connect();

            //get files
            if (system_message[0] == 'I')
            {
                PrintMessage.PrintSM(system_message, ConsoleColor.Cyan, true);

                string? fileName;
                do
                {
                    //enter path
                    PrintMessage.PrintSM("Please, enter file's directory: ", ConsoleColor.White, false);
                    string? path = Console.ReadLine();
                    if (path != null)
                    {
                        if (path[path.Length - 1] != '\\')
                        {
                            path += '\\';
                        }
                    }
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
                    system_message = pccClient.GetFile(path, new FileInfo(fileName));

                    //print
                    if (system_message[0] == 'I')
                    {
                        PrintMessage.PrintSM(system_message, ConsoleColor.White, true);
                    }
                    if (system_message[0] == 'W')
                    {
                        PrintMessage.PrintSM(system_message, ConsoleColor.Yellow, true);
                    }
                    if ((system_message[0] == 'E') || (system_message[0] == 'F')) 
                    {
                        PrintMessage.PrintSM(system_message, ConsoleColor.Red, true);
                    }
                } while ((system_message[0] == 'I') || (system_message[0] == 'W'));

                //disconnect
                PrintMessage.PrintSM(pccClient.Disconnect(), ConsoleColor.Yellow, true);
            }
            else
            {
                PrintMessage.PrintSM(system_message, ConsoleColor.Red, true);
            }
            Console.ReadLine();
        }
    }
}