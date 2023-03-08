using System.Collections.Generic;

namespace SketchPen.Compose.Services.Absraction;

public interface IComposerService
{
    string Name { get; }

    ComposeResult Compose(string path,
                          IEnumerable<int> sizes,
                          IEnumerable<string> customGlobals,
                          IEnumerable<float>? dpiList = null);

    string FileExtension { get; }
}
