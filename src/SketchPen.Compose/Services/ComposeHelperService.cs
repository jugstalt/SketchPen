using Microsoft.Extensions.Options;
using SketchPen.Plot;
using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;

namespace SketchPen.Compose.Services;

internal class ComposeHelperService
{
    private readonly CompilerService _compiler;
    private readonly ComposeHelperServiceOptions _options;

    public ComposeHelperService(CompilerService compiler,
                                IOptions<ComposeHelperServiceOptions> options)
    {
        _compiler = compiler;
        _options = options.Value;
    }

    public byte[] ComposeImage(IEnumerable<string> filenames,
                               int size,
                               string globals)
    {
        return ComposeImage(filenames, size, globals, (name, x, y) => { }).data;
    }

    public (byte[] data, int imageWidth, int imageHeight)
                               ComposeImage(IEnumerable<string> filenames, 
                               int size, 
                               string globals,
                               Action<string, int, int> insertSprite)
    {
        int matrixX = (int)Math.Sqrt(filenames.Count());
        int matrixY = (int)Math.Ceiling((double)filenames.Count() / matrixX);

        int imageWidth = matrixX * size;
        int imageHeight = matrixY * size;

        using (var plotContext = (IPlotContext)Activator.CreateInstance(_options.PlotContextType))
        {
            plotContext.Init(imageWidth, imageHeight, PlotContextOrigin.UpperLeft);
           
            int index = 0;
            foreach (var filename in filenames)
            {
                var code = _compiler.PreCompile(filename, globals);
                var commands = _compiler.Compile(code);

                using (var filePlotContext = (IPlotContext)Activator.CreateInstance(_options.PlotContextType))
                {
                    filePlotContext.Init(size, size);

                    foreach (var command in commands)
                    {
                        command.Execute(filePlotContext);
                    }

                    int posX = (index % matrixX) * size;
                    int posY = (index / matrixX) * size;
                    string name = new FileInfo(filename).Name;
                    name = name.Substring(0, name.LastIndexOf('.'));
                    
                    plotContext.Canvas.DrawImage(filePlotContext,
                        new CanvasRectangle(0, 0, size, size),
                        new CanvasRectangle(posX,
                                            posY,
                                            size, size));

                    insertSprite?.Invoke(name, posX, posY);
                }

                index++;
            }

            return (plotContext.Encode(EncodeFormat.Png), imageWidth, imageHeight);
        }
    }
}
