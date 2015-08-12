namespace Document.AWS
{
    using System;

    public static class Display
    {
        public static void Message(string message)
        {
            MessageEx(message);
        }

        public static void Error(string errorMessage)
        {
            MessageEx(errorMessage, ConsoleColor.Red);
        }

        private static void MessageEx(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message + Environment.NewLine);
            Console.ResetColor();
        }
    }
}
