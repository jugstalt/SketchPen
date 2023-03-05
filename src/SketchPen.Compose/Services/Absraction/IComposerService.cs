using System;
using System.Collections.Generic;
using System.Text;

namespace SketchPen.Compose.Services.Absraction;

public interface IComposerService
{
    ComposeResult Compose(string path,
                          IEnumerable<int> sizes,
                          IEnumerable<string> customGlobals);

    string ContentType { get; }
}
