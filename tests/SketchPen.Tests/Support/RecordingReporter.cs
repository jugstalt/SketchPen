using SketchPen.Output;

namespace SketchPen.Tests.Support;

/// <summary>An <see cref="IConsoleReporter"/> that just remembers what happened.</summary>
public sealed class RecordingReporter : IConsoleReporter
{
    public List<string> FilesWritten { get; } = new();
    public List<string> Errors { get; } = new();
    public bool? Success { get; private set; }

    public void Usage() { }
    public void PlotStart(string fileName) { }
    public void PlotProgress(string token) { }
    public void PlotDone() { }
    public void FileWritten(string path) => FilesWritten.Add(path);
    public void SyntaxError(string? codeFile, string message, string? statement) => Errors.Add($"syntax: {message}");
    public void GenericError(string message, string? stackTrace) => Errors.Add($"error: {message}");
    public void Complete(bool success) => Success = success;
}
