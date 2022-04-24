namespace SketchPen.Code.Extensions
{
    static public class IOExtensions
    {
        static public bool HasAllowedExtension(this FileInfo fi)
        {
            switch(fi.Extension.ToLower())
            {
                case ".sp":
                case ".spt":
                case ".globals":
                    return true;
            }

            return false;
        }
    }
}
