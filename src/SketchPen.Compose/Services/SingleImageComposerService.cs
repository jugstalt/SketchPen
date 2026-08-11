using Microsoft.Extensions.Options;
using SketchPen.Compose.Extensions;
using SketchPen.Compose.Services.Absraction;
using SketchPen.Plot;
using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SketchPen.Compose.Services;

internal class SingleImageComposerService : IComposerService
{
    private readonly ComposeHelperService _composerHelper;

    public SingleImageComposerService(ComposeHelperService composeHelper)
    {
        _composerHelper = composeHelper;
    }

    public string Id => "png";

    public string Name => "Image";

    public ComposeResult Compose(string id, 
                                 string path,
                                 IEnumerable<int> sizes,
                                 IEnumerable<string> customGlobals,
                                 IEnumerable<float>? dpiList = null)
    {
        if (sizes.Count() != 1)
        {
            throw new ArgumentException("Only one size possible to compose an image");
        }
        if (customGlobals.Count() != 1)
        {
            throw new ArgumentException("Only one globals possible to compose an image");
        }

        var size = sizes.First();
        var globals = customGlobals.First();

        return new ComposeResult()
        {
            Data = _composerHelper.ComposeImage(path.CollectFilenames("*.sp"), size, globals)
        };
    }

    public string FileExtension => "png";
}
