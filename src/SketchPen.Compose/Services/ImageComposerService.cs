using Microsoft.Extensions.Options;
using SketchPen.Compose.Services.Absraction;
using SketchPen.Plot;
using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SketchPen.Compose.Services;

internal class ImageComposerService : IComposerService
{
    private readonly ComposeHelperService _composerHelper;

    public ImageComposerService(ComposeHelperService composeHelper)
    {
        _composerHelper = composeHelper;
    }

    public ComposeResult Compose(string path,
                                 IEnumerable<int> sizes,
                                 IEnumerable<string> customGlobals)
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

        List<string> filenames = new List<string>();    

        if (Directory.Exists(path))
        {
            foreach (var file in Directory.GetFiles(path, "*.sp"))
            {
                filenames.Add(file);
            }
        }
        else
        {
            filenames.Add(path);
        }

        return new ComposeResult()
        {
            Data = _composerHelper.ComposeImage(filenames, size, globals)
        };
    }

    public string ContentType => "image/png";
}
