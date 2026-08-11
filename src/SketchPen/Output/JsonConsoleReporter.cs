using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SketchPen.Output;

/// <summary>
/// Buffers a run's results and emits exactly one JSON document to stdout when
/// <see cref="Complete"/> is called (selected via <c>--output json</c>) — meant for tooling
/// (e.g. a future editor extension) that invokes SketchPen.exe and wants a single structured
/// result instead of scraping text. Progress ("Plot x...", "...16@1") is intentionally not
/// streamed in this mode, to keep stdout parseable as a single JSON value. Exit codes are
/// unaffected by this mode — 0 on success, 1 on failure, same as <see cref="TextConsoleReporter"/>.
/// </summary>
public class JsonConsoleReporter : IConsoleReporter
{
    private readonly List<string> _filesWritten = new();
    private JsonRunError? _error;
    private bool _usageOnly;

    public void Usage()
    {
        _usageOnly = true;
    }

    public void PlotStart(string fileName)
    {
        // Intentionally not streamed — see class remarks.
    }

    public void PlotProgress(string token)
    {
        // Intentionally not streamed — see class remarks.
    }

    public void PlotDone()
    {
        // Intentionally not streamed — see class remarks.
    }

    public void FileWritten(string path)
    {
        _filesWritten.Add(path);
    }

    public void SyntaxError(string? codeFile, string message, string? statement)
    {
        _error = new JsonRunError("syntax", message, codeFile, statement);
    }

    public void GenericError(string message, string? stackTrace)
    {
        _error = new JsonRunError("generic", message, null, null);
    }

    public void Complete(bool success)
    {
        if (_usageOnly)
        {
            Console.WriteLine(JsonSerializer.Serialize(
                new JsonRunResult(true, Array.Empty<string>(), null, "Usage: SketchPen.exe path [options]"),
                JsonReporterContext.Default.JsonRunResult));
            return;
        }

        Console.WriteLine(JsonSerializer.Serialize(
            new JsonRunResult(success, _filesWritten.ToArray(), _error, null),
            JsonReporterContext.Default.JsonRunResult));
    }
}

internal record JsonRunResult(
    bool Success,
    string[] FilesWritten,
    JsonRunError? Error,
    string? Message);

internal record JsonRunError(
    string Type,
    string Message,
    string? CodeFile,
    string? Statement);

[JsonSourceGenerationOptions(WriteIndented = false, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(JsonRunResult))]
internal partial class JsonReporterContext : JsonSerializerContext;
