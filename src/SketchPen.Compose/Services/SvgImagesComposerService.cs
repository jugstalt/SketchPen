using SketchPen.Compose.Extensions;
using SketchPen.Compose.Services.Absraction;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace SketchPen.Compose.Services;

/// <summary>
/// Batch-exports every .sp file of a set, in every requested style, as an individual
/// SVG inside a zip. Unlike <see cref="ImagesComposerService"/> this deliberately does
/// NOT loop over sizes or dpi/resolutions: SVG is resolution-independent, so looping
/// either would just emit byte-identical files. The (optional) requested sizes are only
/// used to pick a single reference pixel width — <see cref="Enumerable.Max"/> of the
/// given sizes, or <see cref="DefaultReferenceSize"/> if none were given — that feeds
/// pen-width min/max clamping (see SvgPlotContext/PlotContext.CreatePen); it has no
/// effect on the SVG's inherent scalability.
/// </summary>
internal class SvgImagesComposerService : IComposerService
{
    private const int DefaultReferenceSize = 64;

    private readonly SvgComposeHelperService _composerHelper;

    public SvgImagesComposerService(SvgComposeHelperService composeHelper)
    {
        _composerHelper = composeHelper;
    }

    public string Id => "svg-zip";

    public string Name => "Images (SVG)";

    public string FileExtension => "zip";

    public ComposeResult Compose(string id,
                                 string path,
                                 IEnumerable<int> sizes,
                                 IEnumerable<string> customGlobals,
                                 IEnumerable<float>? dpiList = null)
    {
        int size = sizes != null && sizes.Any() ? sizes.Max() : DefaultReferenceSize;

        using var ms = new MemoryStream();
        using (var zipArchive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            foreach (var globals in customGlobals)
            {
                foreach (string filename in path.CollectFilenames("*.sp"))
                {
                    string svgFilename = $"sketchpen-{filename.FileTitle()}.svg";

                    var svgData = _composerHelper.ComposeSvg(filename, size, globals);

                    var svgEntry = zipArchive.CreateEntry($"{globals.OrTake("default")}/{svgFilename}");
                    using (var svgEntryStream = svgEntry.Open())
                    {
                        svgEntryStream.Write(svgData);
                    }
                }
            }
        }

        return new ComposeResult()
        {
            Data = ms.ToArray()
        };
    }
}
