using SketchPen.Compose.Extensions;
using SketchPen.Compose.Services.Absraction;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Text;

namespace SketchPen.Compose.Services;

internal class ImagesComposerService : IComposerService
{
    private readonly ComposeHelperService _composerHelper;

    public ImagesComposerService(ComposeHelperService composeHelper)
    {
        _composerHelper = composeHelper;
    }

    public string Name => "Images";

    public string FileExtension => "zip";

    public ComposeResult Compose(string id,
                                 string path,
                                 IEnumerable<int> sizes, 
                                 IEnumerable<string> customGlobals, 
                                 IEnumerable<float>? dpiList = null)
    {
        using var ms = new MemoryStream();
        using (var zipArchive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            foreach (var globals in customGlobals)
            {
                foreach (string filename in path.CollectFilenames("*.sp"))
                {
                    foreach (var size in sizes)
                    {
                        foreach (var dpi in dpiList.OrSingle(96f))
                        {
                            string imageFilename = $"sketchpen-{filename.FileTitle()}.png";

                            float dpiFactor = dpi / 96f;

                            var imageData = _composerHelper.ComposeImage(new[] { filename }, (int)(size * dpiFactor), globals);

                            var imgEntry = zipArchive.CreateEntry($"{globals.OrTake("default")}/{size}/{dpi.ToResolutionsFolder()}/{imageFilename}");
                            using (var imgEntryStream = imgEntry.Open())
                            {
                                imgEntryStream.Write(imageData);
                            }
                        }
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
