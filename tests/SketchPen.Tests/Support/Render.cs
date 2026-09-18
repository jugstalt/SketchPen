using System.Text;
using SketchPen.Plot;
using SketchPen.Plot.Services;
using SketchPen.Plot.Skia;

namespace SketchPen.Tests.Support;

/// <summary>
/// Renders icon scripts through the real pipeline (PreComplier -> lexer -> commands ->
/// Skia SVG context), the same way the CLI does. SVG is used for assertions because it is text and
/// deterministic across platforms, unlike PNG bytes.
/// </summary>
public static class Render
{
    // CommandTypesService scans the assemblies for command types -- build it once.
    public static readonly CommandTypesService CommandTypes = new();

    /// <summary>Renders an existing .sp file as SVG at the given reference size.</summary>
    public static string SvgOfFile(string spFile, string style = "", int size = 128)
    {
        var plotter = new Plotter(CommandTypes, typeof(SvgPlotContext));
        plotter.Init(spFile, style);
        return Encoding.UTF8.GetString(plotter.Plot(size, size, EncodeFormat.Svg));
    }

    /// <summary>
    /// Renders <paramref name="spCode"/> (written to a temp icon set) as SVG. Optional
    /// <paramref name="defaultGlobals"/> becomes styles/default.globals; <paramref name="extraFiles"/>
    /// are written relative to the set's root (templates, extra styles, ...).
    /// </summary>
    public static string Svg(
        string spCode,
        string? defaultGlobals = null,
        string style = "",
        IDictionary<string, string>? extraFiles = null,
        int size = 128)
    {
        using var set = new TempIconSet();
        if (defaultGlobals != null)
        {
            set.Write("styles/default.globals", defaultGlobals);
        }
        foreach (var (path, content) in extraFiles ?? new Dictionary<string, string>())
        {
            set.Write(path, content);
        }

        var file = set.Write("icon.sp", spCode);
        return SvgOfFile(file, style, size);
    }

    /// <summary>Line endings differ per platform/git config; compare text, not bytes.</summary>
    public static string Normalize(string text) => text.Replace("\r\n", "\n").TrimEnd();
}
