using SketchPen.Plot;
using SketchPen.Plot.Services;
using SketchPen.Plot.Skia;
using System.Collections.Generic;

namespace SketchPen.Compose.Services;

/// <summary>
/// Compiles and renders a single .sp file to SVG bytes. Deliberately bypasses
/// <see cref="ComposeHelperService"/> (which is raster/bitmap-sprite-sheet specific) and
/// hardcodes <see cref="SvgPlotContext"/> — there is exactly one SVG rendering backend,
/// so there is no need for the generic <see cref="ComposeHelperServiceOptions.PlotContextType"/>
/// indirection that the raster composers use.
/// </summary>
internal class SvgComposeHelperService
{
    private readonly CompilerService _compiler;

    public SvgComposeHelperService(CompilerService compiler)
    {
        _compiler = compiler;
    }

    public byte[] ComposeSvg(string filename, int size, string globals)
    {
        return ComposeSvgWithGlobals(filename, size, globals).svg;
    }

    /// <summary>
    /// Same as <see cref="ComposeSvg"/>, but also returns the resolved
    /// <c>globals.set</c>/<c>tryset</c> key-value pairs that were in effect while
    /// rendering — the same values <c>@@name</c> resolved to (see <c>GlobalsCommand</c>),
    /// no separate re-parsing of the .globals file needed.
    /// </summary>
    public (byte[] svg, IDictionary<string, object> globals) ComposeSvgWithGlobals(
        string filename, int size, string globals)
    {
        var code = _compiler.PreCompile(filename, globals);
        var commands = _compiler.Compile(code);

        using (var plotContext = new SvgPlotContext())
        {
            plotContext.Init(size, size);

            foreach (var command in commands)
            {
                command.Execute(plotContext);
            }

            return (plotContext.Encode(EncodeFormat.Svg), plotContext.Globals);
        }
    }
}
