using SketchPen.Plot;
using SketchPen.Plot.Exceptions;
using SketchPen.Plot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SketchPen;

class Program
{
    static int Main(string[] args)
    {
        try
        {
            string path = args.FirstOrDefault();
            List<int> sizes = new List<int>();
            string outFolder = String.Empty;
            string customGlobalsName = String.Empty;
            string format = "png";

            for (int i = 1; i < args.Length - 1; i++)
            {
                switch (args[i])
                {
                    case "-outfolder":
                        outFolder = args[++i];
                        break;
                    case "-custom_globals":
                        customGlobalsName = args[++i];
                        break;
                    case "-format":
                        format = args[++i].ToLowerInvariant();
                        break;
                }
            }

            if (format != "png" && format != "svg")
            {
                throw new Exception($"Unsupported -format '{format}'. Supported formats: png, svg");
            }

            if (String.IsNullOrEmpty(path))
            {
                Console.WriteLine("Usage: SketchPen.exe path [options]");
                return 0;
            }

            #region Collect filenames

            List<string> fileNames = new List<string>();
            if (new FileInfo(path).Exists)
            {
                fileNames.Add(path);
            }
            else if (new DirectoryInfo(path).Exists)
            {
                fileNames.AddRange(new DirectoryInfo(path).GetFiles("*.sp").Select(fi => fi.FullName));
            }
            else
            {
                throw new Exception($"Can't find part of the path '{path}'");
            }

            #endregion

            if (sizes.Count() == 0)
            {
                sizes.AddRange(new int[] { 16, 26, 32, 64, 128 });
            }

            if (!String.IsNullOrEmpty(outFolder))
            {
                outFolder = outFolder + "/";
            }

            var commandTypes = new CommandTypesService();

            Type plotContextType = format == "svg" ?
                typeof(SketchPen.Plot.Skia.SvgPlotContext) :
                typeof(SketchPen.Plot.Skia.PlotContext);

            // SVG is resolution-independent: one file per icon, at a single reference
            // size (used only to scale pen-width min/max clamping), no @1/@2/@3 axis.
            const int SvgReferenceSize = 128;

            foreach (var fileName in fileNames)
            {
                var fileInfo = new FileInfo(fileName);
                string baseName = fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf("."));
                Console.WriteLine($"Plot {fileInfo.Name}...");

                var plotter = new Plotter(commandTypes, plotContextType);

                plotter.Init(fileName, customGlobalsName);

                if (format == "svg")
                {
                    Console.Write($"...svg");

                    var imageData = plotter.Plot(SvgReferenceSize, SvgReferenceSize, EncodeFormat.Svg);

                    var targetFileInfo = new FileInfo($"{outFolder}{baseName}.svg");
                    if (!targetFileInfo.Directory.Exists)
                    {
                        targetFileInfo.Directory.Create();
                    }

                    File.WriteAllBytes(targetFileInfo.FullName, imageData);
                }
                else
                {
                    foreach (var size in sizes)
                    {
                        for (int ratio = 1; ratio <= 3; ratio++)
                        {
                            string targetFile = $"{baseName}_{size}@{ratio}.png";
                            Console.Write($"...{size}@{ratio}");

                            var imageData = plotter.Plot(size * ratio, size * ratio);

                            var targetFileInfo = new FileInfo($"{outFolder}{targetFile}");
                            if (!targetFileInfo.Directory.Exists)
                            {
                                targetFileInfo.Directory.Create();
                            }

                            File.WriteAllBytes(targetFileInfo.FullName, imageData);
                        }
                    }
                }

                Console.WriteLine("...done");
            }

            return 0;
        }
        catch (SyntaxErrorException see)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(see.CodeFile);
            Console.WriteLine($"ERROR: {see.Message}");
            Console.WriteLine(">>");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($">> {see.Statement}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(">>");

            return 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"Exception: {ex.Message}");
#if DEBUG
            Console.WriteLine("Stacktrace:");
            Console.WriteLine(ex.StackTrace);
#endif
            return 1;
        }
    }
}
