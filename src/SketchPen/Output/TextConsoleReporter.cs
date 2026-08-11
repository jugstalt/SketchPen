using System;

namespace SketchPen.Output;

/// <summary>
/// Reproduces the CLI's original plain-text console output exactly, line for line, so any
/// existing script that parses `SketchPen.exe`'s stdout keeps working unchanged. This is the
/// default reporter (<c>--output text</c>, or no <c>--output</c> flag at all).
/// </summary>
public class TextConsoleReporter : IConsoleReporter
{
    public void Usage()
    {
        Console.WriteLine("Usage: SketchPen.exe path [options]");
    }

    public void PlotStart(string fileName)
    {
        Console.WriteLine($"Plot {fileName}...");
    }

    public void PlotProgress(string token)
    {
        Console.Write($"...{token}");
    }

    public void PlotDone()
    {
        Console.WriteLine("...done");
    }

    public void FileWritten(string path)
    {
        // Not printed in text mode today — individual output file paths were never part of
        // the original console output, only the "...{size}@{ratio}"/"...done" progress dots.
    }

    public void SyntaxError(string? codeFile, string message, string? statement)
    {
        Console.WriteLine(Environment.NewLine);
        Console.WriteLine(codeFile);
        Console.WriteLine($"ERROR: {message}");
        Console.WriteLine(">>");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($">> {statement}");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(">>");
    }

    public void GenericError(string message, string? stackTrace)
    {
        Console.WriteLine(Environment.NewLine);
        Console.WriteLine($"Exception: {message}");
#if DEBUG
        Console.WriteLine("Stacktrace:");
        Console.WriteLine(stackTrace);
#endif
    }

    public void Complete(bool success)
    {
        // No-op: text mode already streamed everything live, there is no final summary today.
    }
}
