using SketchPen.Compose.Extensions;
using SketchPen.Compose.Services.Absraction;
using SketchPen.Plot;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;

namespace SketchPen.Compose.Services;

/// <summary>
/// Batch-exports every .sp file of a set, in every requested style, as an individual,
/// self-contained HTML page inside a zip — mirrors <see cref="SvgImagesComposerService"/>
/// (one entry per icon, no per-size/per-dpi loop — see its remarks for why) but each entry
/// is an HTML page instead of a plain SVG: colors sourced from a known <c>.globals</c>
/// variable are replaced with CSS custom property references
/// (<c>var(--sketchpen-&lt;name&gt;, &lt;original&gt;)</c>), and the page includes a matching
/// <c>:root</c> block, the substituted SVG inline (required for the CSS variables to
/// resolve — see the note emitted on the page itself), a copy-paste-ready escaped snippet,
/// and one color picker per variable to prove the live restyling works.
/// </summary>
internal class SvgCssVariableComposerService : IComposerService
{
    private const int DefaultReferenceSize = 64;

    private readonly SvgComposeHelperService _composerHelper;

    public SvgCssVariableComposerService(SvgComposeHelperService composeHelper)
    {
        _composerHelper = composeHelper;
    }

    public string Id => "svg-vars-zip";

    public string Name => "Images (CSS Variables)";

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
                    var (svgBytes, resolvedGlobals) = _composerHelper.ComposeSvgWithGlobals(filename, size, globals);
                    var svg = Encoding.UTF8.GetString(svgBytes);

                    var (substitutedSvg, usedVariables) = SvgColorVariableSubstitution.Substitute(svg, resolvedGlobals);

                    var html = BuildHtml(filename.FileTitle(), substitutedSvg, usedVariables);

                    string htmlFilename = $"sketchpen-{filename.FileTitle()}.html";
                    var htmlEntry = zipArchive.CreateEntry($"{globals.OrTake("default")}/{htmlFilename}");
                    using (var htmlEntryStream = htmlEntry.Open())
                    {
                        htmlEntryStream.Write(Encoding.UTF8.GetBytes(html));
                    }
                }
            }
        }

        return new ComposeResult()
        {
            Data = ms.ToArray()
        };
    }

    private static string BuildHtml(string iconName,
                                    string svg,
                                    IReadOnlyList<SvgColorVariableSubstitution.UsedVariable> usedVariables)
    {
        var rootVars = new StringBuilder();
        var controls = new StringBuilder();

        foreach (var usedVariable in usedVariables)
        {
            rootVars.AppendLine($"      --sketchpen-{usedVariable.Key}: {usedVariable.OriginalValue};");

            var color = PlotColor.FromHtml(usedVariable.OriginalValue);
            string hex = $"#{color.R:x2}{color.G:x2}{color.B:x2}";

            controls.AppendLine($"""
                <label>
                  <input type="color" value="{hex}" oninput="document.documentElement.style.setProperty('--sketchpen-{usedVariable.Key}', this.value)">
                  {WebUtility.HtmlEncode(usedVariable.Key)}
                </label>
                """);
        }

        string escapedSvg = WebUtility.HtmlEncode(svg);
        string encodedIconName = WebUtility.HtmlEncode(iconName);

        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <title>{{encodedIconName}} — themeable SVG</title>
              <style>
                :root {
            {{rootVars}}    }
                body { font-family: system-ui, sans-serif; margin: 2rem; background: #f7f7f7; color: #222; }
                h1 { font-size: 1.2rem; }
                .note { font-size: 13px; color: #666; max-width: 640px; }
                .note code { background: #eee; padding: 0 .25em; border-radius: 3px; }
                .preview { display: flex; align-items: flex-start; gap: 2rem; margin: 1.5rem 0; flex-wrap: wrap; }
                .icon-box { width: 160px; height: 160px; display: flex; align-items: center; justify-content: center; background: #fff; border: 1px solid #ddd; border-radius: 8px; flex: none; }
                .icon-box svg { width: 128px; height: 128px; }
                .controls label { display: flex; align-items: center; gap: .5rem; margin-bottom: .5rem; font-size: 14px; }
                pre { background: #1e1e1e; color: #d4d4d4; padding: 1rem; border-radius: 8px; overflow: auto; font-size: 12px; max-width: 100%; }
              </style>
            </head>
            <body>
              <h1>{{encodedIconName}}</h1>
              <p class="note">
                This icon's colors are wired to CSS custom properties (<code>--sketchpen-*</code>,
                defined in <code>:root</code> above). Copy the inline <code>&lt;svg&gt;</code>
                snippet below directly into your own HTML — it must stay <strong>inline</strong>
                in the DOM for this to work; an <code>&lt;img src="icon.svg"&gt;</code> or CSS
                <code>background-image</code> reference lives in an isolated document and cannot
                see this page's CSS variables. Set the <code>--sketchpen-*</code> variables on
                any ancestor element to restyle it.
              </p>
              <div class="preview">
                <div class="icon-box">
            {{svg}}
                </div>
                <div class="controls">
            {{controls}}    </div>
              </div>
              <pre><code>{{escapedSvg}}</code></pre>
            </body>
            </html>
            """;
    }
}
