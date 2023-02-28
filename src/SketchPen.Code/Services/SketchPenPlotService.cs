using Microsoft.Extensions.Options;
using SketchPen.Plot;
using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Compile;

namespace SketchPen.Code.Services;

public class SketchPenPlotService
{
    private readonly SketchPenCodeServiceOptions _options;

    public SketchPenPlotService(IOptionsMonitor<SketchPenCodeServiceOptions> optionsMonitor)
    {
        _options = optionsMonitor.CurrentValue;
    }

    public byte[] Plot(int width, int height, string route, string globals)
    {
        var compiler = new Compiler();
        var code = compiler.PreCompile(Path.Combine(_options.RootPath, route), globals);

        var commands = compiler.Compile(code);

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
}
