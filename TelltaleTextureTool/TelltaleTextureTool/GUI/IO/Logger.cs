using System;
using System.IO;

namespace TelltaleTextureTool;

// A simple logger class that logs exceptions to a text file
public class Logger
{
    static readonly string CrashesDirectory = Path.Combine(
        Directory.GetCurrentDirectory(),
        "CrashLogs"
    );

    private static Logger? _instance;

    public static Logger Instance() => _instance ??= new Logger();

    static Logger()
    {
        // Create the "Crashes" directory if it doesn't exist
        Directory.CreateDirectory(CrashesDirectory);
    }

    public static void Log(Exception e)
    {
        // Create a new text file with the current date as the file name
        string logFileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
        string logFilePath = Path.Combine(CrashesDirectory, logFileName);

        using (StreamWriter writer = new(logFilePath))
        {
            writer.WriteLine("Timestamp: " + DateTime.Now);
            writer.WriteLine("Error Details: " + e.Message);
            writer.WriteLine("Stack Trace:\n" + e.StackTrace);
            writer.WriteLine("------------------------------------------");
            writer.Close();
        }

        Console.WriteLine("Error Details: " + e.Message);
        Console.WriteLine("Stack Trace:\n" + e.StackTrace);
        Console.WriteLine("------------------------------------------");
    }
}
