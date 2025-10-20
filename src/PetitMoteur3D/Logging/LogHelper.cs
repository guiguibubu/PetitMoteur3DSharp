using System;

namespace PetitMoteur3D.Logging
{
    internal static class LogHelper
    {
        public static void Log(string? format, params object?[] args)
        {
            Log(string.Format(format ?? "", args));
        }

        public static void Log(string? message)
        {
            System.Diagnostics.Trace.WriteLine(message);
        }

        public static void Log(Exception ex)
        {
            Exception currentEx = ex;
            bool logFinished = false;
            do
            {
                Log(ex.Message);
                Log(ex.StackTrace);
                if (currentEx.InnerException is not null)
                {
                    currentEx = currentEx.InnerException;
                }
                else
                {
                    logFinished = true;
                }
            } while (!logFinished);
        }
    }
}
