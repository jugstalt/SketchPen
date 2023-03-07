using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SketchPen.Compose.Extensions;

static internal class StringExtensions
{
    static public string OrTake(this string input, string output)
    {
        if (string.IsNullOrEmpty(input))
        {
            return output;
        }

        return input;
    }

    static public IEnumerable<string> CollectFilenames(this string path, string filter)
    {
        List<string> filenames = new List<string>();

        if (Directory.Exists(path))
        {
            foreach (var file in Directory.GetFiles(path, "*.sp"))
            {
                filenames.Add(file);
            }
        }
        else
        {
            filenames.Add(path);
        }

        return filenames.ToArray();
    }

    static public string FileTitle(this string filename)
    {
        var fi=new FileInfo(filename);

        return fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length).ToLower();
    }
}
