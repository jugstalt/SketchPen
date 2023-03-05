using System.Collections.Generic;

namespace SketchPen.Compose.Services.Absraction;

public interface IComposerService
{
    ComposeResult Compose(string path,
                          IEnumerable<int> sizes,
                          IEnumerable<string> customGlobals,
                          IEnumerable<float>? dpiList = null);

    string ContentType { get; }
}
