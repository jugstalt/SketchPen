using SketchPen.Plot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SketchPen
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                string path = args.FirstOrDefault();
                List<int> sizes = new List<int>();
                string outFolder = String.Empty;

                for (int i = 1; i < args.Length - 1; i++)
                {
                    switch (args[i])
                    {
                        case "-outfolder":
                            outFolder = args[++i];
                            break;
                    }
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
                    fileNames.Append(path);
                }
                else if (new DirectoryInfo(path).Exists)
                {
                    fileNames.AddRange(new DirectoryInfo(path).GetFiles("*.sp").Select(fi => fi.FullName));
                }
                else
                {
                    throw new Exception($"Can't find part of the path '{ path }'");
                }

                #endregion

                if (sizes.Count() == 0)
                {
                    sizes.AddRange(new int[] { 16, 26, 32, 64, 128, 256, 512 });
                }

                if (!String.IsNullOrEmpty(outFolder))
                {
                    outFolder = outFolder + "/";
                }

                foreach (var fileName in fileNames)
                {
                    var fileInfo = new FileInfo(fileName);
                    Console.WriteLine($"Plot { fileInfo.Name }...");

                    foreach (var size in sizes)
                    {
                        string targetFile = $"{ fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf(".")) }_{ size }.png";
                        Console.Write(targetFile);

                        var plotter = new Plotter(size, size);
                        var imageData = plotter.Plot(fileName);

                        var targetFileInfo = new FileInfo($"{ outFolder }{ targetFile }");
                        if (!targetFileInfo.Directory.Exists)
                        {
                            targetFileInfo.Directory.Create();
                        }

                        File.WriteAllBytes(targetFileInfo.FullName, imageData);

                        Console.WriteLine("...done");
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine($"Exception: { ex.Message }");
#if DEBUG
                Console.WriteLine("Stacktrace:");
                Console.WriteLine(ex.Message);
#endif
                return 1;
            }
        }
    }
}
