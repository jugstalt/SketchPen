using SketchPen.Compose.Extensions;
using SketchPen.Compose.Services.Absraction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SketchPen.Compose.Services;

internal class SvgSingleImageComposerService : IComposerService
{
    private readonly SvgComposeHelperService _composerHelper;

    public SvgSingleImageComposerService(SvgComposeHelperService composeHelper)
    {
        _composerHelper = composeHelper;
    }

    public string Name => "Vector Image";

    public string FileExtension => "svg";

    public ComposeResult Compose(string id,
                                 string path,
                                 IEnumerable<int> sizes,
                                 IEnumerable<string> customGlobals,
                                 IEnumerable<float>? dpiList = null)
    {
        if (sizes.Count() != 1)
        {
            throw new ArgumentException("Only one size possible to compose a vector image");
        }
        if (customGlobals.Count() != 1)
        {
            throw new ArgumentException("Only one globals possible to compose a vector image");
        }

        var filenames = path.CollectFilenames("*.sp");
        if (filenames.Count() != 1)
        {
            throw new ArgumentException("Only one .sp file possible to compose a vector image");
        }

        var size = sizes.First();
        var globals = customGlobals.First();

        return new ComposeResult()
        {
            Data = _composerHelper.ComposeSvg(filenames.First(), size, globals)
        };
    }
}
