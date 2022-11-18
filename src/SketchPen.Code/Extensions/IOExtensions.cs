using System.Text.RegularExpressions;

namespace SketchPen.Code.Extensions
{
    static public class IOExtensions
    {
        static public bool HasAllowedExtension(this FileInfo fi)
        {
            switch (fi.Extension.ToLower())
            {
                case ".sp":
                case ".spt":
                case ".globals":
                    return true;
            }

            return false;
        }

        static public bool IsValidFilename(this string filename)
        {
            Regex containsABadCharacter = new Regex($"[{Regex.Escape(new string(System.IO.Path.GetInvalidPathChars()))}]");
            if (containsABadCharacter.IsMatch(filename))
            {
                return false;
            }

            // other checks for UNC, drive-path format, etc

            return true;
        }

        static public string ToRelativeFilePath(this string path, string rootPath)
        {
            return path?.Substring(rootPath.Length + 1).Replace(@"\", "/") ?? String.Empty;
        }

        static public bool IsValidGlobalsName(this string name)
        {
            return !String.IsNullOrEmpty(name) &&
                !name.ContainsAny(new[] {'/','\\'}) &&
                name.StartsWith("_") &&
                name.EndsWith(".globals");
        }
    }               
}
