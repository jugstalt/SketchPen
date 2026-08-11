using System.Collections.Generic;

namespace SketchPen.Compose.Services.Absraction;

public interface IComposerService
{
    /// <summary>
    /// Short, stable, CLI/API-friendly identifier (e.g. "svg-zip") for selecting this
    /// composer programmatically. Unlike <see cref="Name"/> (a display label) or the CLR
    /// type name, this is meant to be a safe, human-typable value for a command-line flag
    /// or query parameter.
    /// </summary>
    string Id { get; }

    string Name { get; }

    ComposeResult Compose(string id,
                          string path,
                          IEnumerable<int> sizes,
                          IEnumerable<string> customGlobals,
                          IEnumerable<float>? dpiList = null);

    string FileExtension { get; }
}
