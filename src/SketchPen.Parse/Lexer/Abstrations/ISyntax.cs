namespace SketchPen.Parse.Lexer.Abstrations;

public interface ISyntax
{
    string[] Keywords { get; }
    string[] Separator { get; }
    string[] Comments { get; }
    string[] Operators { get; }
}
