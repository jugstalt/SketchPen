using SketchPen.Plot;
using SketchPen.Plot.Compile;
using SketchPen.Plot.Debugging;
using SketchPen.Plot.Models;
using SketchPen.Plot.Services;
using SketchPen.Plot.Services.Abstraction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SketchPen.Commands;

/// <summary>
/// New <c>language-info</c> subcommand — a machine-consumption-only command (always emits one
/// JSON document, no human text mode) exposing the same completion grammar and per-project
/// <c>@@variable</c> names the web app's Monaco editor already gets, for tooling such as the
/// VS Code extension. Reuses <see cref="IEditorLanguageService"/> (already attribute-driven,
/// zero new grammar knowledge) and mirrors
/// <c>SketchPenCodeService.TryGetGlobalVariableNames</c>'s exact behavior for the dynamic part
/// (including silently returning an empty list, never failing the command, on a broken
/// <c>default.globals</c>).
/// </summary>
public class LanguageInfoCommandHandler
{
    private readonly IEnumerable<IEditorLanguageService> _editorLanguages;
    private readonly CompilerService _compiler;

    public LanguageInfoCommandHandler(IEnumerable<IEditorLanguageService> editorLanguages,
                                      CompilerService compiler)
    {
        _editorLanguages = editorLanguages;
        _compiler = compiler;
    }

    public int Execute(string? path)
    {
        if (!string.IsNullOrEmpty(path) &&
            !new DirectoryInfo(path).Exists &&
            !new FileInfo(path).Exists)
        {
            WriteJson(new LanguageInfoResult(false, null, null, null, $"Can't find part of the path '{path}'"));
            return 1;
        }

        var commands = new Dictionary<string, IDictionary<string, IEnumerable<EditorCompletionModel>>>
        {
            ["code"] = GetCompletion(EditorFileType.Code),
            ["template"] = GetCompletion(EditorFileType.Template),
            ["globals"] = GetCompletion(EditorFileType.Globals)
        };

        IEnumerable<string>? globalVariables = string.IsNullOrEmpty(path)
            ? null
            : TryGetGlobalVariableNames(path);

        IEnumerable<StyleInfo>? styles = string.IsNullOrEmpty(path)
            ? null
            : GetAvailableStyles(path);

        WriteJson(new LanguageInfoResult(true, commands, globalVariables, styles, null));
        return 0;
    }

    private IDictionary<string, IEnumerable<EditorCompletionModel>> GetCompletion(EditorFileType fileType)
    {
        var service = _editorLanguages.FirstOrDefault(l => l.MatchEditorFileType(fileType));

        return service?.EditorCompletion ?? new Dictionary<string, IEnumerable<EditorCompletionModel>>();
    }

    // Mirrors SketchPenCodeService.TryGetGlobalVariableNames (src/SketchPen.Code/Services/SketchPenCodeService.cs)
    // exactly: compiles and executes default.globals against a DebugPlotContext and reads back
    // the resulting Globals dictionary's keys. Any failure (missing/broken default.globals, or no
    // styles folder at all) yields an empty list, never an error for this command. The styles
    // folder is resolved the same way PreComplier resolves it for any .sp file in this directory
    // (SketchPenConfigResolver: nearest ancestor .sketchpen.json if present, else "./styles").
    // appendGlobals: false because we're compiling the globals file itself, not a script that
    // needs it prepended.
    private IEnumerable<string> TryGetGlobalVariableNames(string path)
    {
        string directory = new DirectoryInfo(path).Exists
            ? path
            : new FileInfo(path).DirectoryName ?? path;

        string stylesFolder = SketchPenConfigResolver.ResolveStylesPath(directory);
        string globalsFile = Path.Combine(stylesFolder, "default.globals");
        if (!File.Exists(globalsFile))
        {
            return Array.Empty<string>();
        }

        try
        {
            var code = _compiler.PreCompile(globalsFile, appendGlobals: false);
            var commands = _compiler.Compile(code);

            using (var plotContext = new DebugPlotContext())
            {
                plotContext.Init(0, 0);

                foreach (var command in commands)
                {
                    command.Execute(plotContext);
                }

                return plotContext.Globals.Keys.Distinct().Order();
            }
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    // Every named style's .globals file in the resolved styles folder (default.globals itself
    // excluded -- it's not selectable via -style, it's always loaded), paired with its display
    // label from the nearest ancestor .sketchpen.json's "styles" metadata if present, else just
    // the style name itself. Same "never fail the command" spirit as TryGetGlobalVariableNames --
    // a missing/empty styles folder just yields an empty list.
    private IEnumerable<StyleInfo> GetAvailableStyles(string path)
    {
        string directory = new DirectoryInfo(path).Exists
            ? path
            : new FileInfo(path).DirectoryName ?? path;

        var (stylesFolder, resolvedConfig) = SketchPenConfigResolver.ResolveStylesPathWithConfig(directory);
        if (!Directory.Exists(stylesFolder))
        {
            return Array.Empty<StyleInfo>();
        }

        var labels = resolvedConfig?.Config.Styles;

        return new DirectoryInfo(stylesFolder)
            .GetFiles("*.globals")
            .Select(fi => Path.GetFileNameWithoutExtension(fi.Name))
            .Where(name => !string.Equals(name, "default", StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .Select(name => new StyleInfo(
                name,
                labels != null && labels.TryGetValue(name, out var meta) && !string.IsNullOrWhiteSpace(meta.Label)
                    ? meta.Label!
                    : name))
            .ToArray();
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static void WriteJson(LanguageInfoResult result)
    {
        Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
    }

    private record LanguageInfoResult(
        bool Success,
        IDictionary<string, IDictionary<string, IEnumerable<EditorCompletionModel>>>? Commands,
        IEnumerable<string>? GlobalVariables,
        IEnumerable<StyleInfo>? Styles,
        string? Error);

    private record StyleInfo(string Name, string Label);
}
