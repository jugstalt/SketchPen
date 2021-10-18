using SketchPen.Parse.Lexer;
using SketchPen.Plot.Extensions;
using System.Reflection;
using System.Linq;
using SketchPen.Plot.Abstraction;
using System.Collections.Generic;
using System;
using SketchPen.Plot.Reflection;
using System.Drawing;
using System.IO;
using SketchPen.Plot.Compile;

namespace SketchPen.Plot
{
    public class Plotter
    {
        #region Static Constructor & Fields

        static internal IEnumerable<Type> PlotCommandTypes = null;
        static internal Color TransparentColor = Color.Transparent; // Color.FromArgb(1, 0, 0);
        static internal System.Drawing.Drawing2D.SmoothingMode DefaultSmothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        private const int MinPlotSize= 10;

        static Plotter()
        {
            PlotCommandTypes = Assembly.GetAssembly(typeof(Plotter))
                                       .GetTypes()
                                       .Where(t => 
                                                t.IsClass && 
                                                t.GetCustomAttribute<PlotCommandKeywordAttribute>()!=null &&
                                                typeof(IPlotCommand).IsAssignableFrom(t));
        }

        #endregion

        private readonly int _canvasWith, _canvasHeight;

        public Plotter(int canvasWidth, int canvasHeight)
        {
            _canvasWith = canvasWidth;
            _canvasHeight = canvasHeight;
        }

        public byte[] Plot(string fileName, string customGlobalsName="")
        {
            var code = File.ReadAllText(fileName).Trim();

            #region Pre Compile

            var preCompiler = new PreComplier(fileName, customGlobalsName, true);
            code = preCompiler.Compile(fileName);

            //Console.WriteLine(code);

            #endregion

            var syntax = new SketchPenSyntax();
            var lexicalAnalyser = new LexicalAnalyser(syntax);
            var tokens = lexicalAnalyser.Tokenize(code);

            var commands = tokens.GetStatements(syntax)
                                 .GetPlotCommands();

            using (var bitmap = new Bitmap(Math.Max(_canvasWith, MinPlotSize), Math.Max(_canvasHeight, MinPlotSize)))
            {
                bitmap.SetResolution(96f, 96f);
                bitmap.MakeTransparent();

                using (var plotContext = new PlotContext(bitmap))
                {
                    plotContext.GraphicsContext.SmoothingMode = Plotter.DefaultSmothingMode;
                    plotContext.GraphicsContext.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                    //plotContext.GraphicsContext.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    plotContext.GraphicsContext.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    plotContext.GraphicsContext.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                    foreach(var command in commands)
                    {
                        command.Execute(plotContext);
                    }
                }

                var ms = new MemoryStream();
                if (_canvasWith < MinPlotSize)
                {
                    using(var bm = new Bitmap(_canvasWith, _canvasHeight))
                    using (var gr = Graphics.FromImage(bm))
                    {
                        bm.SetResolution(96f, 96f);
                        gr.DrawImage(bitmap, new Rectangle(0, 0, _canvasWith, _canvasHeight),
                                             new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                             GraphicsUnit.Pixel);

                        bm.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
                else
                {
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                }

                return ms.ToArray();
            }
        }
    }
}
