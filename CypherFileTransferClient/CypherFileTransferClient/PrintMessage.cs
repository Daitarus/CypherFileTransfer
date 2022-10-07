using ProtocolCryptographyC;

namespace CypherFileTransferClient
{
    internal static class PrintMessage
    {
        public static void PrintColorMessage(string message, ConsoleColor consoleColor, bool ifNewLine)
        {
            Console.ForegroundColor = consoleColor;
            if (ifNewLine)
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.Write(message);
            }
            Console.ResetColor();
        }

        public static void PrintSystemMessage(PccSystemMessage systemMessage)
        {
            string systemMessageStr;
            if (systemMessage.AdditionalMessage != null)
            {
                systemMessageStr = systemMessage.AdditionalMessage;
                systemMessageStr += $" - {systemMessage.Message}";
            }
            else
            {
                systemMessageStr = systemMessage.Message;
            }

            switch (systemMessage.Key)
            {
                case PccSystemMessageKey.ERROR:
                    {
                        PrintColorMessage(systemMessageStr,ConsoleColor.Red,true);
                        break;
                    }
                case PccSystemMessageKey.INFO:
                    {
                        PrintColorMessage(systemMessageStr, ConsoleColor.White, true);
                        break;
                    }
                case PccSystemMessageKey.WARRNING:
                    {
                        PrintColorMessage(systemMessageStr, ConsoleColor.Yellow, true);
                        break;
                    }
                case PccSystemMessageKey.FATAL_ERROR:
                    {
                        PrintColorMessage(systemMessageStr, ConsoleColor.Red, true); ;
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
