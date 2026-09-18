namespace SketchPen.Tests.Support;

/// <summary>
/// A throw-away directory tree for one test: write icon scripts, templates, globals and
/// .sketchpen.json files into it, point the compiler at them, and it disappears on dispose.
/// </summary>
public sealed class TempIconSet : IDisposable
{
    public TempIconSet()
    {
        Root = Path.Combine(Path.GetTempPath(), "sketchpen-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Root);
    }

    public string Root { get; }

    /// <summary>Full path of <paramref name="relativePath"/> inside this set (forward slashes are fine).</summary>
    public string PathOf(string relativePath) =>
        Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));

    /// <summary>Writes a file (creating folders as needed) and returns its full path.</summary>
    public string Write(string relativePath, string content)
    {
        var full = PathOf(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, content);
        return full;
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(Root, recursive: true);
        }
        catch
        {
            // Best effort -- a locked temp file must not fail the test.
        }
    }
}
