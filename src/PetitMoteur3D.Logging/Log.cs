using System;

namespace PetitMoteur3D.Logging;

public static class Log
{
    /// <summary>
    /// The globally-shared logger.
    /// </summary>
    /// <exception cref="ArgumentNullException">When <paramref name="value"/> is <code>null</code></exception>
    public static Serilog.ILogger Logger
    {
        get => Serilog.Log.Logger;
        set => Serilog.Log.Logger = value;
    }

    public static void Information(string? format, params object?[] args)
    {
        Information(string.Format(format ?? "", args));
    }

    public static void Information(string? message)
    {
        if (message is not null)
        {
            Serilog.Log.Information(message);
        }
    }

    public static void Warning(string? format, params object?[] args)
    {
        Warning(string.Format(format ?? "", args));
    }

    public static void Warning(string? message)
    {
        if (message is not null)
        {
            Serilog.Log.Warning(message);
        }
    }

    public static void Error(string? format, params object?[] args)
    {
        Error(string.Format(format ?? "", args));
    }

    public static void Error(string? message)
    {
        if (message is not null)
        {
            Serilog.Log.Error(message);
        }
    }

    public static void Fatal(Exception ex)
    {
        Serilog.Log.Fatal(ex, "Exception");
    }

    public static string GenerateLogFileName()
    {
        return $"logs_{DateTime.Now.ToString("yyyy-MM-dd_HH\\hmm\\mss")}.txt";
    }
}
