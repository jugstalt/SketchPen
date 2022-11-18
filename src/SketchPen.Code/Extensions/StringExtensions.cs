namespace SketchPen.Code.Extensions;

static public class StringExtensions
{
    static public bool ContainsAny(this string str, char[] chars)
    {
        if (String.IsNullOrEmpty(str))
        {
            return false;
        }

        return chars
            .Select(c => str.Contains(c))
            .Where(c => c == true)
            .FirstOrDefault();
    }
}
