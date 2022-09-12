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

            ////enter ip
            //while (!errorEnter)
            //{
            //    PrintMessage.PrintSM("Please, enter server's ip: ", ConsoleColor.White, false);
            //    errorEnter = IPAddress.TryParse(Console.ReadLine(), out ip);
            //    if (!errorEnter)
            //    {
            //        PrintMessage.PrintSM("Error: Incorrect data !!!", ConsoleColor.Red, true);
            //    }
            //}
            //errorEnter = false;
            ////enter port
            //while (!errorEnter)
            //{
            //    PrintMessage.PrintSM("Please, enter tcp port: ", ConsoleColor.White, false);
            //    errorEnter = int.TryParse(Console.ReadLine(), out port);
            //    if (!errorEnter)
            //    {
            //        PrintMessage.PrintSM("Error: Incorrect data !!!", ConsoleColor.Red, true);
            //    }
            //}
            //errorEnter = false;

            IPEndPoint ipPoint = new IPEndPoint(ip, port);
            string login = "admin";
            string password = "pa$$w0rd";
            PccClient pccClient = new PccClient(ipPoint, login, password);
            if(pccClient.Connect())
            {
                pccClient.TransferFile("test.txt");
                Console.WriteLine("OK");
            }
            else
            {
                Console.WriteLine("ERROR");
            }
        }
    }
}