using SketchPen.Compose.Extensions;
using SketchPen.Compose.Services.Absraction;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SketchPen.Compose.Services;

internal class WebSpriteComposerService : IComposerService
{
    private readonly ComposeHelperService _composerHelper;

    public WebSpriteComposerService(ComposeHelperService composeHelper)
    {
        _composerHelper = composeHelper;
    }

    public string Name => "Web Sprites";

    public string FileExtension => "zip";

    // https://localhost:7165/code/package?id=basic&globals=,e,kagis,epz&sizes=16,24,32,64
    public ComposeResult Compose(string path,
                                 IEnumerable<int> sizes,
                                 IEnumerable<string> customGlobals,
                                 IEnumerable<float>? dpiList = null)
    {
        using var ms = new MemoryStream();
        using (var zipArchive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            var htmlLinks = new StringBuilder();
            var htmlBody = new StringBuilder();

            foreach (var globals in customGlobals)
            {
                string cssFileName = $"sketchpen-{globals.OrTake("default")}.css";
                StringBuilder css = new StringBuilder();

                htmlLinks.AppendLine($"""<link rel="stylesheet" href="content/css/{cssFileName}">""");

                foreach (var size in sizes)
                {
                    htmlBody.AppendLine($"<h1>{globals.OrTake("default")}:{size}x{size}<h1>");

                    foreach (var dpi in dpiList.OrSingle(96f))
                    {
                        string imageFilename = $"sketchpen-{globals.OrTake("default")}-{size}@{(int)dpi}.png";
                        StringBuilder iconsCss = new StringBuilder();
                        float dpiFactor = dpi / 96f;

                        var imageData = _composerHelper.ComposeImage(path.CollectFilenames("*.sp"), (int)(size * dpiFactor), globals,
                            (name, x, y) =>
                            {
                                if (dpiFactor == 1f)
                                {
                                    iconsCss.AppendLine($$"""
                                               .sketchpen-icon-{{globals.OrTake("defaut")}}-{{size}}.{{name}} {
                                                    background-position: -{{x}}px -{{y}}px
                                               }
                                               """);
                                    htmlBody.AppendLine($"""<div class="sketchpen-icon-{globals.OrTake("defaut")}-{size} {name}" title="{name}"></div>""");
                                }
                            });

                        if (dpiFactor == 1f)
                        {
                            css.Append(Environment.NewLine);
                            css.Append($$"""
                                   .sketchpen-icon-{{globals.OrTake("defaut")}}-{{size}} {
                                      background-image: url("../img/{{imageFilename}}");
                                      background-repeat: no-repeat;
                                      background-size: {{imageData.imageWidth}}px {{imageData.imageHeight}}px;
                                      width: {{size}}px;
                                      height: {{size}}px;
                                      display: inline-block;
                                  }
                                  """);

                            css.Append(iconsCss);
                        }
                        else
                        {
                            css.Append(Environment.NewLine);
                            css.Append($$"""
                                         @media (-webkit-min-device-pixel-ratio: {{(dpi / 96f).ToInvariantString()}}), 
                                                (min-resolution: {{dpi.ToInvariantString()}}dpi) {
                                         
                                             .sketchpen-icon-{{globals.OrTake("defaut")}}-{{size}} {
                                                 background-image: url("../img/{{imageFilename}}");
                                                 background-size: {{(int)(imageData.imageWidth / dpiFactor)}}px {{(int)(imageData.imageHeight / dpiFactor)}}px;
                                             } 
                                         }
                                         """);
                        }



                        var imgEntry = zipArchive.CreateEntry($"content/img/{imageFilename}");
                        using (var imgEntryStream = imgEntry.Open())
                        {
                            imgEntryStream.Write(imageData.data);
                        }
                    }
                }

                var cssEntry = zipArchive.CreateEntry($"content/css/{cssFileName}");
                using (var cssEntryStream = cssEntry.Open())
                {
                    cssEntryStream.Write(Encoding.UTF8.GetBytes(css.ToString()));
                }
            }

            var htmlEntry = zipArchive.CreateEntry("sketchpen.html");
            using (var htmlEntryStream = htmlEntry.Open())
            {
                htmlEntryStream.Write(Encoding.UTF8.GetBytes($"""
                    <!DOCTYPE html>
                    <html lang="en">
                      <head>
                        <meta charset="UTF-8">
                        <meta name="viewport" content="width=device-width, initial-scale=1.0">
                        <meta http-equiv="X-UA-Compatible" content="ie=edge">
                        <title>Sketchpen</title>
                        {htmlLinks.ToString()}
                      </head>
                      <body>
                    	{htmlBody.ToString()}
                      </body>
                    </html>
                    """));
            }
        }

        return new ComposeResult()
        {
            Data = ms.ToArray()
        };
    }
}
