using System;

namespace TabbySheet
{
    public static class Logger
    {
        private static Action<string> LogAction { get; set; } = Console.WriteLine;

        public static void SetLogAction(Action<string> logAction) => Logger.LogAction = logAction;
        public static void Log(string message) => LogAction?.Invoke(message);
    }
}