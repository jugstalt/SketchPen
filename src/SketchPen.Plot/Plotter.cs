using SketchPen.Parse.Lexer;
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

namespace SketchPen.Plot
{
    public class Plotter
    {
        #region Static Constructor & Fields

        static internal IEnumerable<Type> PlotCommandTypes = null;
        
        private const int MinPlotSize = 10;

        static Plotter()
        {
            PlotCommandTypes = Assembly.GetAssembly(typeof(Plotter))
                                       .GetTypes()
                                       .Where(t =>
                                                t.IsClass &&
                                                t.GetCustomAttribute<PlotCommandKeywordAttribute>() != null &&
                                                typeof(IPlotCommand).IsAssignableFrom(t));
        }

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
            var code = File.ReadAllText(fileName).Trim();

            #region Pre Compile

            var preCompiler = new PreComplier(fileName, customGlobalsName, true);
            code = preCompiler.Compile(fileName);

            //Console.WriteLine();
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

                using (var plotContext = (IPlotContext)Activator.CreateInstance(_plotContextType, new object[] { bitmap }))
                {
                    foreach (var command in commands)
                    {
                        command.Execute(plotContext);
                    }
                }

                var ms = new MemoryStream();
                if (_canvasWith < MinPlotSize)
                {
                    using (var bm = new Bitmap(_canvasWith, _canvasHeight))
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
