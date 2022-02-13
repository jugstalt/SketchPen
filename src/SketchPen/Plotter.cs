using SketchPen.Parse.Lexer;
using SketchPen.Plot;
using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Compile;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SketchPen
{
    public class Plotter
    {
        #region Static Constructor & Fields
        
        private const int MinPlotSize = 10;


        #endregion

        private readonly Type _plotContextType;
        private readonly int _canvasWith, _canvasHeight;

        public Plotter(Type plotContextType, int canvasWidth, int canvasHeight)
        {
            _plotContextType = plotContextType;
            _canvasWith = canvasWidth;
            _canvasHeight = canvasHeight;
        }

        public byte[] Plot(string fileName, string customGlobalsName = "")
        {
            var compiler = new Compiler();
            var code = compiler.PreCompile(fileName, customGlobalsName);

            var commands = compiler.Compile(code);

            using (var plotContext = (IPlotContext)Activator.CreateInstance(_plotContextType))
            {
                plotContext.Init(Math.Max(_canvasWith, MinPlotSize), Math.Max(_canvasHeight, MinPlotSize));

                foreach (var command in commands)
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

                return plotContext.Encode(EncodeFormat.Png);
            }
        }
    }
}
