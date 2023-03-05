using Microsoft.Extensions.Options;
using SketchPen.Plot;
using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Services;

namespace SketchPen.Code.Services;

public class SketchPenPlotService
{
    private readonly CompilerService _compiler;
    private readonly SketchPenCodeServiceOptions _options;

    public SketchPenPlotService(CompilerService compiler,
                                IOptionsMonitor<SketchPenCodeServiceOptions> optionsMonitor)
    {
        _compiler = compiler;
        _options = optionsMonitor.CurrentValue;
    }

    public byte[] Plot(int width, int height, string route, string globals)
    {
        var code = _compiler.PreCompile(Path.Combine(_options.RootPath, route), globals);

        var commands = _compiler.Compile(code);

        using (var plotContext = (IPlotContext?)Activator.CreateInstance(typeof(SketchPen.Plot.Skia.PlotContext)))
        {
            plotContext!.Init(width, height);

            foreach (var command in commands)
            {
                command.Execute(plotContext);
            }

            return plotContext.Encode(EncodeFormat.Png);
        }
    }

    public string RootPath => _options.RootPath;
}
