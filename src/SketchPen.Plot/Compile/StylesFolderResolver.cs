using System.IO;
using System.Text.Json;

namespace SketchPen.Plot.Compile;

/// <summary>
/// Deserialized shape of an icon set's optional <c>.sketchpen.json</c> config file. Only one key
/// for now: <c>stylesPath</c>, resolved relative to the config file's own directory (mirroring
/// how <c>#include</c> resolves relative paths in <see cref="PreComplier"/>).
/// </summary>
/// <remarks>
/// A plain mutable class rather than a C# record: this project targets netstandard2.1, which
/// lacks the <c>System.Runtime.CompilerServices.IsExternalInit</c> type records need for their
/// init-only setters, and pulling in a polyfill package for one tiny DTO isn't worth it.
/// </remarks>
public sealed class SketchPenConfig
{
    public string? StylesPath { get; set; }
}

/// <summary>
/// Resolves the folder a <c>.sp</c> file's globals/styles live in. Every icon-set directory may
/// carry a <c>.sketchpen.json</c> naming a <c>stylesPath</c> — which can point anywhere,
/// including a location shared by multiple icon sets (e.g. several sets all pointing at one
/// <c>plot/styles</c> folder). Without a config file (or with one that omits <c>stylesPath</c>),
/// the styles folder defaults to a local <c>styles</c> subfolder.
/// </summary>
public static class StylesFolderResolver
{
    private const string ConfigFileName = ".sketchpen.json";
    private const string DefaultStylesSubfolder = "styles";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Given the directory containing a .sp/.spt/.globals file, returns the absolute path to
    /// that folder's styles directory. Malformed JSON in <c>.sketchpen.json</c> is swallowed and
    /// falls back to the default — same "optional file, never hard-fail on it" spirit as a
    /// missing globals file elsewhere in this pipeline. The returned path is not checked for
    /// existence; callers check individual files (matching existing behavior).
    /// </summary>
    public static string Resolve(string scriptDirectory)
    {
        var configFile = new FileInfo(Path.Combine(scriptDirectory, ConfigFileName));
        if (configFile.Exists)
        {
            try
            {
                var json = File.ReadAllText(configFile.FullName);
                var config = JsonSerializer.Deserialize<SketchPenConfig>(json, JsonOptions);
                if (!string.IsNullOrWhiteSpace(config?.StylesPath))
                {
                    return Path.GetFullPath(Path.Combine(scriptDirectory, config.StylesPath));
                }
            }
            catch (JsonException)
            {
                // Fall through to the default below.
            }
        }

        return Path.GetFullPath(Path.Combine(scriptDirectory, DefaultStylesSubfolder));
    }
}
