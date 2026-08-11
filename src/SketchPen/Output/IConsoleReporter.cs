namespace SketchPen.Output;

/// <summary>
/// Reports render/compose progress and results. Two implementations: <see cref="TextConsoleReporter"/>
/// (default, byte-for-byte compatible with the CLI's original plain-text console output) and
/// <see cref="JsonConsoleReporter"/> (machine-readable, selected via <c>--output json</c>), so
/// external tooling (e.g. a future editor extension) can invoke SketchPen.exe and parse a single
/// structured result instead of scraping text.
/// </summary>
public interface IConsoleReporter
{
    /// <summary>No path/command given — prints the short usage line.</summary>
    void Usage();

    /// <summary>Starting to render a single .sp file.</summary>
    void PlotStart(string fileName);

    /// <summary>One size/ratio (e.g. "16@1") or format (e.g. "svg") of the current file was rendered.</summary>
    void PlotProgress(string token);

    /// <summary>The current file finished rendering (all sizes/ratios).</summary>
    void PlotDone();

    /// <summary>An output file was written to disk.</summary>
    void FileWritten(string path);

    /// <summary>A .sp/.spt/.globals compile-time syntax error occurred (fatal, run stops here).</summary>
    void SyntaxError(string? codeFile, string message, string? statement);

    /// <summary>Any other fatal error occurred (fatal, run stops here).</summary>
    void GenericError(string message, string? stackTrace);

    /// <summary>Call exactly once, right before returning the process exit code.</summary>
    void Complete(bool success);
}
