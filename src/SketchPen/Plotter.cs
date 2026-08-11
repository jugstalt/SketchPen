using SketchPen.Plot;
using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Compile;
using SketchPen.Plot.Services;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SketchPen;

public class Plotter
{
    #region Static Constructor & Fields

    private const int MinPlotSize = 10;

    #endregion

    private readonly CommandTypesService _commandTypes;
    private readonly Type _plotContextType;
    private IEnumerable<IPlotCommand> _commands = null!;

    public Plotter(CommandTypesService commandTypes,
                   Type plotContextType)
    {
        _commandTypes = commandTypes;
        _plotContextType = plotContextType;
    }

    public void Init(string fileName, string customGlobalsName = "")
    {
        var compiler = new CompilerService(_commandTypes);
        var code = compiler.PreCompile(fileName, customGlobalsName);

        _commands = compiler.Compile(code);
    }

    public byte[] Plot(int canvasWidth, int canvasHeight, EncodeFormat format = EncodeFormat.Png)
    {
        using (var plotContext = (IPlotContext)Activator.CreateInstance(_plotContextType)!)
        {
            plotContext.Init(Math.Max(canvasWidth, MinPlotSize), Math.Max(canvasHeight, MinPlotSize));

            foreach (var command in _commands)
            {
                command.Execute(plotContext);
            }

            //var ms = new MemoryStream();
            //if (_canvasWith < MinPlotSize)
            //{
            //    using (var bm = new Bitmap(_canvasWith, _canvasHeight))
            //    using (var gr = Graphics.FromImage(bm))
            //    {
            //        bm.SetResolution(96f, 96f);
            //        gr.DrawImage(bitmap, new Rectangle(0, 0, _canvasWith, _canvasHeight),
            //                             new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            //                             GraphicsUnit.Pixel);

            //        bm.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            //    }
            //}
            //else
            //{
            //    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            //}

            return plotContext.Encode(format);
        }
    }
}
