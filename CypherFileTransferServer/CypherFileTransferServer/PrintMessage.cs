using ProtocolCryptographyC;
using NLog;

namespace CypherFileTransferServer
{
    internal static class PrintMessage
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
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
                        logger.Error(systemMessageStr);
                        break;
                    }
                case PccSystemMessageKey.INFO:
                    {
                        logger.Info(systemMessageStr);
                        break;
                    }
                case PccSystemMessageKey.WARRNING:
                    {
                        logger.Warn(systemMessageStr);
                        break;
                    }
                case PccSystemMessageKey.FATAL_ERROR:
                    {
                        logger.Fatal(systemMessageStr);
                        break;
                    }
                default:
                    break;
            }
        }
    }
}
