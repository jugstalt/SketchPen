using SketchPen.Output;
using SketchPen.Plot;
using SketchPen.Plot.Compile;
using SketchPen.Plot.Exceptions;
using SketchPen.Plot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SketchPen.Commands;

/// <summary>
/// The original, legacy CLI behavior — <c>SketchPen.exe &lt;path&gt; [-outfolder ...]
/// [-style ...] [-format png|svg]</c> — moved unchanged into a DI-constructed class so
/// it can be invoked as the <see cref="System.CommandLine.RootCommand"/>'s own action. This is a
/// structural move only: file collection, the (now optionally overridable, default
/// <c>16,26,32,64,128 × @1,2,3</c>) PNG loop, the single-SVG-per-icon branch, and error handling
/// are an exact behavioral port of the original <c>Program.cs</c>/<c>Main</c> body — output
/// files, console text, and exit codes are unchanged when <c>-sizes</c>/<c>-resolutions</c> are
/// left at their defaults.
/// </summary>
public class RenderCommandHandler
{
    private const int SvgReferenceSize = 128;

    private static readonly int[] DefaultSizes = { 16, 26, 32, 64, 128 };
    private static readonly int[] DefaultResolutions = { 1, 2, 3 };

    private readonly CommandTypesService _commandTypes;

    public RenderCommandHandler(CommandTypesService commandTypes)
    {
        _commandTypes = commandTypes;
    }

    public int Execute(string? path, string outFolder, string styleName, string format,
                       string? sizes, string? resolutions, IConsoleReporter reporter)
    {
        try
        {
            format = string.IsNullOrEmpty(format) ? "png" : format.ToLowerInvariant();

            if (format != "png" && format != "svg")
            {
                throw new Exception($"Unsupported -format '{format}'. Supported formats: png, svg");
            }

            if (string.IsNullOrEmpty(path))
            {
                reporter.Usage();
                reporter.Complete(true);
                return 0;
            }

            // .sketchpen.json defaults (nearest ancestor of <path>'s own directory) fill in
            // whatever the caller didn't pass explicitly -- an explicit CLI flag always wins.
            // Safe to resolve even if <path> turns out to be invalid: FindConfig just walks
            // upward checking for a file at each level, no exception either way; the actual
            // "does <path> exist" check below still raises its own clear error.
            string configDir = new FileInfo(path).Exists ? new FileInfo(path).Directory!.FullName : path;
            var resolvedConfig = SketchPenConfigResolver.FindConfig(configDir);

            if (string.IsNullOrEmpty(styleName) && !string.IsNullOrWhiteSpace(resolvedConfig?.Config.DefaultStyle))
            {
                styleName = resolvedConfig!.Config.DefaultStyle!;
            }

            var sizeList = ParsePositiveInts(sizes, "-sizes") ?? resolvedConfig?.Config.DefaultSizes ?? DefaultSizes;
            var resolutionList = ParsePositiveInts(resolutions, "-resolutions") ?? resolvedConfig?.Config.DefaultResolutions ?? DefaultResolutions;

            #region Collect filenames

            List<string> fileNames = new List<string>();
            if (new FileInfo(path).Exists)
            {
                fileNames.Add(path);
            }
            else if (new DirectoryInfo(path).Exists)
            {
                fileNames.AddRange(new DirectoryInfo(path).GetFiles("*.sp").Select(fi => fi.FullName));
            }
            else
            {
                throw new Exception($"Can't find part of the path '{path}'");
            }

            #endregion

            if (string.IsNullOrEmpty(outFolder) && !string.IsNullOrWhiteSpace(resolvedConfig?.Config.OutFolder))
            {
                outFolder = Path.GetFullPath(Path.Combine(resolvedConfig!.ConfigDirectory, resolvedConfig.Config.OutFolder!));
            }

            if (!string.IsNullOrEmpty(outFolder))
            {
                outFolder = outFolder + "/";
            }

            Type plotContextType = format == "svg"
                ? typeof(SketchPen.Plot.Skia.SvgPlotContext)
                : typeof(SketchPen.Plot.Skia.PlotContext);

            foreach (var fileName in fileNames)
            {
                var fileInfo = new FileInfo(fileName);
                string baseName = fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf("."));
                reporter.PlotStart(fileInfo.Name);

                var plotter = new Plotter(_commandTypes, plotContextType);
                plotter.Init(fileName, styleName);

                if (format == "svg")
                {
                    reporter.PlotProgress("svg");

                    var imageData = plotter.Plot(SvgReferenceSize, SvgReferenceSize, EncodeFormat.Svg);

                    var targetFileInfo = new FileInfo($"{outFolder}{baseName}.svg");
                    if (!targetFileInfo.Directory!.Exists)
                    {
                        targetFileInfo.Directory.Create();
                    }

                    File.WriteAllBytes(targetFileInfo.FullName, imageData);
                    reporter.FileWritten(targetFileInfo.FullName);
                }
                else
                {
                    foreach (var size in sizeList)
                    {
                        foreach (var ratio in resolutionList)
                        {
                            string targetFile = $"{baseName}_{size}@{ratio}.png";
                            reporter.PlotProgress($"{size}@{ratio}");

                            var imageData = plotter.Plot(size * ratio, size * ratio);

                            var targetFileInfo = new FileInfo($"{outFolder}{targetFile}");
                            if (!targetFileInfo.Directory!.Exists)
                            {
                                targetFileInfo.Directory.Create();
                            }

                            File.WriteAllBytes(targetFileInfo.FullName, imageData);
                            reporter.FileWritten(targetFileInfo.FullName);
                        }
                    }
                }

                reporter.PlotDone();
            }

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

    // Missing/empty -sizes/-resolutions returns null so the caller can fall back to the classic
    // fixed defaults -- an explicit, deliberately-chosen list of positive integers, or nothing.
    private static int[]? ParsePositiveInts(string? csv, string optionName)
    {
        if (string.IsNullOrEmpty(csv))
        {
            return null;
        }

        var values = csv.Split(',').Select(s => s.Trim()).Select(s =>
        {
            if (!int.TryParse(s, out int value) || value <= 0)
            {
                throw new Exception($"Invalid {optionName} value '{s}' — expected a comma-separated list of positive integers, e.g. \"16,32,64\".");
            }
            return value;
        }).ToArray();

        return values;
    }
}
