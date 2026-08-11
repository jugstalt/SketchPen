using SketchPen.Compose.Services.Absraction;
using SketchPen.Output;
using SketchPen.Plot.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SketchPen.Commands;

/// <summary>
/// New <c>compose</c> subcommand — invokes any registered <see cref="IComposerService"/>
/// directly from the command line (the same abstraction that powers every export mode in the
/// web app), selected by its short <see cref="IComposerService.Id"/> rather than the web app's
/// full CLR type name.
/// </summary>
public class ComposeCommandHandler
{
    private readonly IEnumerable<IComposerService> _composers;

    public ComposeCommandHandler(IEnumerable<IComposerService> composers)
    {
        _composers = composers;
    }

    public int Execute(string? path, string? composerId, string? sizes, string? styles, string? resolutions,
                       string? outFile, IConsoleReporter reporter)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new Exception(
                    "Missing required <path> argument. Note: <path> goes right after 'compose', " +
                    "e.g. 'SketchPen.exe compose plot/webgis --composer svg-zip' — not before it. " +
                    "Run 'SketchPen.exe compose --help' for the full syntax and examples.");
            }
            if (string.IsNullOrEmpty(composerId))
            {
                throw new Exception(
                    "Missing required --composer <id>. Run 'SketchPen.exe compose --help' to see " +
                    "the list of valid ids and example commands.");
            }

            var composer = _composers.FirstOrDefault(c => string.Equals(c.Id, composerId, StringComparison.OrdinalIgnoreCase));
            if (composer == null)
            {
                var validIds = string.Join(", ", _composers.Select(c => c.Id).OrderBy(id => id));
                throw new Exception($"Unknown composer '{composerId}'. Valid ids: {validIds}");
            }

            var sizeList = ParseInts(sizes);
            var styleList = ParseStyles(styles);
            var dpiList = ParseFloats(resolutions);

            string id = Path.GetFileName(path.TrimEnd('/', '\\'));

            var result = composer.Compose(id, path, sizeList, styleList, dpiList);

            string targetPath = string.IsNullOrEmpty(outFile)
                ? $"{id}.{composer.FileExtension}"
                : outFile;

            var targetFileInfo = new FileInfo(targetPath);
            if (targetFileInfo.Directory != null && !targetFileInfo.Directory.Exists)
            {
                targetFileInfo.Directory.Create();
            }

            File.WriteAllBytes(targetFileInfo.FullName, result.Data ?? Array.Empty<byte>());
            reporter.FileWritten(targetFileInfo.FullName);

            reporter.Complete(true);
            return 0;
        }
        catch (SyntaxErrorException see)
        {
            reporter.SyntaxError(see.CodeFile, see.Message, see.Statement);
            reporter.Complete(false);
            return 1;
        }
        catch (Exception ex)
        {
            reporter.GenericError(ex.Message, ex.StackTrace);
            reporter.Complete(false);
            return 1;
        }
    }

    // Missing/empty -sizes: pass through empty rather than inventing a default — composers that
    // require exactly one size (e.g. "svg", "png") already raise a clear error in that case; batch
    // composers already have their own sensible internal defaulting (see SvgImagesComposerService).
    private static IEnumerable<int> ParseInts(string? csv) =>
        string.IsNullOrEmpty(csv) ? Array.Empty<int>() : csv.Split(',').Select(s => int.Parse(s.Trim()));

    // Missing/empty --styles means "just the default style" — [""] — matching what the web app's
    // Package endpoint effectively does for an empty styles query parameter ("".Split(',') == [""]).
    private static IEnumerable<string> ParseStyles(string? csv) =>
        string.IsNullOrEmpty(csv) ? new[] { "" } : csv.Split(',').Select(s => s.Trim().ToLowerInvariant());

    // Missing --resolutions stays null (not an empty list) to match IComposerService.Compose's own
    // "dpiList = null" default and each composer's own null-handling (96 dpi / ignored, depending
    // on composer).
    private static IEnumerable<float>? ParseFloats(string? csv) =>
        string.IsNullOrEmpty(csv) ? null : csv.Split(',').Select(s => float.Parse(s.Trim()));
}
