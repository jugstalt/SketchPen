using Microsoft.Extensions.Options;
using SketchPen.Plot;
using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Services;
using System.Text;

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

    async public Task<string> WriteTempFile(string title, string extension, byte[] data)
    {
        string fileName = $"{title}.{extension}@{CreateRandomTempFileNameExtension()}";
        string filePath = Path.Combine(GetOrCreateTempDirectory(), fileName);

        await File.WriteAllBytesAsync(filePath, data);

        return fileName;
    }

    async public Task<(string name, byte[] data)> ReadTempFile(string tempFilename)
    {
        if (tempFilename.Replace("\\", "/").Contains("/"))  // avoid /subdir/.. or /../../system/..
        {
            throw new ArgumentException("Not allowed Path", nameof(tempFilename));
        }

        var fi = new FileInfo(Path.Combine(GetOrCreateTempDirectory(), tempFilename));
        if(!fi.Exists)
        {
            throw new FileNotFoundException(tempFilename);
        }

        var data = await File.ReadAllBytesAsync(fi.FullName);
        fi.Delete();

        return (tempFilename.Split("@").First(), data);
    }

    #region Helper

    private string GetOrCreateTempDirectory()
    {
        DirectoryInfo directory = new DirectoryInfo(Path.Combine(_options.RootPath, "_temp"));

        if (!directory.Exists)
        {
            directory.Create();
        }

        return directory.FullName;
    }

    private string CreateRandomTempFileNameExtension()
    {
        StringBuilder sb = new();

        for (int i = 0; i < 3; i++)
        {
            sb.Append(Guid.NewGuid().ToString("N"));
        }

        return sb.ToString();
    }

    #endregion
}
