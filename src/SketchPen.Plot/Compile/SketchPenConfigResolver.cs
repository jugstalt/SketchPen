using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SketchPen.Plot.Compile;

/// <summary>
/// Deserialized shape of an icon set's optional <c>.sketchpen.json</c> config file. All keys are
/// optional; an icon-set folder only needs to set the ones it wants to override.
/// </summary>
/// <remarks>
/// A plain mutable class rather than a C# record: this project targets netstandard2.1, which
/// lacks the <c>System.Runtime.CompilerServices.IsExternalInit</c> type records need for their
/// init-only setters, and pulling in a polyfill package for one small set of DTOs isn't worth it.
/// </remarks>
public sealed class SketchPenConfig
{
    /// <summary>Where this folder's styles/globals live, resolved relative to this config file's
    /// own directory. Defaults to "styles" (a local subfolder) if unset and no ancestor config
    /// sets it either.</summary>
    public string? StylesPath { get; set; }

    /// <summary>Style name used when <c>-style</c>/<c>--styles</c> isn't given explicitly.</summary>
    public string? DefaultStyle { get; set; }

    /// <summary>Root command PNG sizes used when <c>-sizes</c> isn't given explicitly.</summary>
    public int[]? DefaultSizes { get; set; }

    /// <summary>Root command @&lt;ratio&gt; resolutions used when <c>-resolutions</c> isn't given
    /// explicitly.</summary>
    public int[]? DefaultResolutions { get; set; }

    /// <summary>Root command output folder used when <c>-outfolder</c> isn't given explicitly,
    /// resolved relative to this config file's own directory.</summary>
    public string? OutFolder { get; set; }

    /// <summary>Human-readable metadata for named styles, keyed by style name (the same name used
    /// with <c>-style</c>/<c>--styles</c>, i.e. the style's <c>.globals</c> filename without the
    /// extension) -- purely descriptive, consumed by tooling (language-info, the VS Code
    /// extension's style dropdowns), never by the compiler itself.</summary>
    public Dictionary<string, StyleMetadata>? Styles { get; set; }

    /// <summary>Named, reusable <c>compose</c> presets (composer id + sizes/styles/resolutions),
    /// selected via <c>compose --profile &lt;name&gt;</c> instead of repeating the same flags every
    /// time.</summary>
    public Dictionary<string, ExportProfile>? ExportProfiles { get; set; }
}

public sealed class StyleMetadata
{
    /// <summary>Display name shown in place of the raw style/file name, e.g. "Dark Mode" for
    /// "bg-dark".</summary>
    public string? Label { get; set; }
}

public sealed class ExportProfile
{
    /// <summary>Composer id (see IComposerService.Id), e.g. "png-zip", "svg-vars-zip".</summary>
    public string? Composer { get; set; }

    /// <summary>Comma-separated sizes, same syntax as <c>compose --sizes</c>.</summary>
    public string? Sizes { get; set; }

    /// <summary>Comma-separated style names, same syntax as <c>compose --styles</c>.</summary>
    public string? Styles { get; set; }

    /// <summary>Comma-separated DPI values, same syntax as <c>compose --resolutions</c>.</summary>
    public string? Resolutions { get; set; }
}

/// <summary>A found <c>.sketchpen.json</c> plus the directory it was found in -- needed alongside
/// the parsed config itself since every path-shaped key (<see cref="SketchPenConfig.StylesPath"/>,
/// <see cref="SketchPenConfig.OutFolder"/>) is resolved relative to wherever the config file
/// actually lives, not necessarily the folder that inherited it (see
/// <see cref="SketchPenConfigResolver.FindConfig"/>).</summary>
public sealed class ResolvedConfig
{
    public ResolvedConfig(SketchPenConfig config, string configDirectory)
    {
        Config = config;
        ConfigDirectory = configDirectory;
    }

    public SketchPenConfig Config { get; }
    public string ConfigDirectory { get; }
}

/// <summary>
/// Resolves an icon-set folder's effective <c>.sketchpen.json</c> config, including
/// <c>styles</c>/<c>globals</c> file locations. Every icon-set directory may carry its own
/// <c>.sketchpen.json</c>; a directory without one inherits the nearest ancestor's instead (the
/// same folder walked up by <c>#include</c>-style relative resolution, one level at a time) --
/// e.g. a single <c>plot/.sketchpen.json</c> can supply defaults for every icon set under
/// <c>plot/</c> that doesn't define its own. The nearest config wins in full; there is no
/// per-key merging across levels.
/// </summary>
public static class SketchPenConfigResolver
{
    private const string ConfigFileName = ".sketchpen.json";
    private const string DefaultStylesSubfolder = "styles";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Walks upward from <paramref name="scriptDirectory"/> (inclusive), returning the nearest
    /// <c>.sketchpen.json</c> found. Malformed JSON at a given level is treated the same as no
    /// file there and the walk continues upward. Returns <c>null</c> if none is found anywhere up
    /// to the filesystem root.
    /// </summary>
    public static ResolvedConfig? FindConfig(string scriptDirectory)
    {
        string? dir = Path.GetFullPath(scriptDirectory);

        while (!string.IsNullOrEmpty(dir))
        {
            var configFile = new FileInfo(Path.Combine(dir, ConfigFileName));
            if (configFile.Exists)
            {
                try
                {
                    var json = File.ReadAllText(configFile.FullName);
                    var config = JsonSerializer.Deserialize<SketchPenConfig>(json, JsonOptions);
                    if (config != null)
                    {
                        return new ResolvedConfig(config, dir);
                    }
                }
                catch (JsonException)
                {
                    // Malformed .sketchpen.json at this level: treat it as absent and keep
                    // walking upward, same "never hard-fail on optional config" spirit as a
                    // missing globals file elsewhere in this pipeline.
                }
            }

            dir = Path.GetDirectoryName(dir);
        }

        return null;
    }

    /// <summary>
    /// Given the directory containing a .sp/.spt/.globals file, returns the absolute path to
    /// that folder's styles directory: the nearest ancestor config's <c>stylesPath</c> (resolved
    /// relative to that config's own directory) if one is found and non-empty, otherwise
    /// "&lt;scriptDirectory&gt;/styles". The returned path is not checked for existence; callers
    /// check individual files (matching existing behavior).
    /// </summary>
    public static string ResolveStylesPath(string scriptDirectory) =>
        ResolveStylesPathWithConfig(scriptDirectory).StylesFolder;

    /// <summary>
    /// Same resolution as <see cref="ResolveStylesPath"/>, but also returns whichever
    /// <see cref="ResolvedConfig"/> was used to get there (or <c>null</c> if none was found
    /// anywhere up the tree) -- for callers that also need other keys from that same config
    /// (e.g. <see cref="SketchPenConfig.Styles"/> display-name metadata), so they don't have to
    /// walk the tree a second time via a separate <see cref="FindConfig"/> call.
    /// </summary>
    public static (string StylesFolder, ResolvedConfig? Config) ResolveStylesPathWithConfig(string scriptDirectory)
    {
        var resolved = FindConfig(scriptDirectory);
        if (resolved != null && !string.IsNullOrWhiteSpace(resolved.Config.StylesPath))
        {
            return (Path.GetFullPath(Path.Combine(resolved.ConfigDirectory, resolved.Config.StylesPath)), resolved);
        }

        return (Path.GetFullPath(Path.Combine(scriptDirectory, DefaultStylesSubfolder)), resolved);
    }
}
