using SketchPen.Parse.Lexer;
using SketchPen.Plot.Exceptions;
using SketchPen.Tests.Support;

namespace SketchPen.Tests;

public class LexerTests
{
    private static List<Token> Tokenize(string text) =>
        new LexicalAnalyser(new SketchPenSyntax()).Tokenize(text).ToList();

    [Fact]
    public void Statement_IsTokenizedIntoKeywordMethodAndParameters()
    {
        var tokens = Tokenize("circle.fill(70,\"#4a90d9\");\r\n");

        Assert.Contains(tokens, t => t.TokenType == TokenType.Keyword && t.TokenValue == "circle");
        Assert.Contains(tokens, t => t.TokenType == TokenType.LiteralConstant && t.TokenValue == "#4a90d9");
        Assert.Contains(tokens, t => t.TokenType == TokenType.NumericalConstant && t.TokenValue == "70");
    }

    // ---- comments: their text must never be lexed as code ----------------------------------

    [Theory]
    [InlineData("// it's a full-line comment;\r\ncircle.draw(40);\r\n")]
    [InlineData("circle.draw(40);// trailing comment, it's fine;\r\n")]
    [InlineData("// a \"quoted\" word and a ; semicolon and an unbalanced ' quote;\r\ncircle.draw(40);\r\n")]
    [InlineData("// don't;\r\n// won't;\r\ncircle.draw(40);\r\n")]
    public void Comments_WithQuotesAndApostrophes_AreNotLexedAsCode(string text)
    {
        var tokens = Tokenize(text);

        Assert.DoesNotContain(tokens, t => t.TokenType == TokenType.CharacterConstant);
        Assert.Contains(tokens, t => t.TokenType == TokenType.Keyword && t.TokenValue == "circle");
    }

    [Fact]
    public void ApostropheOutsideAComment_StillFailsLexing()
    {
        // A char constant is real syntax outside comments -- an unterminated one is an error.
        Assert.ThrowsAny<Exception>(() => Tokenize("pen.color('x;\r\n"));
    }

    [Fact]
    public void StringLiteral_ContainingDoubleSlash_IsOneLiteralNotAComment()
    {
        var tokens = Tokenize("text.draw(\"http://example.org\",10,0,0);\r\n");

        Assert.Contains(tokens, t => t.TokenType == TokenType.LiteralConstant && t.TokenValue == "http://example.org");
        Assert.Contains(tokens, t => t.TokenType == TokenType.NumericalConstant && t.TokenValue == "10");
    }

    // ---- the same through the whole pipeline ------------------------------------------------

    [Fact]
    public void EndToEnd_CommentsWithApostrophes_RenderNormally()
    {
        var svg = Render.Svg(
            "// it's a test\n" +
            "pen.color(\"#000\");\n" +
            "pen.width(2); // don't panic\n" +
            "circle.draw(40); // a \"quoted\" word; and a semicolon\n");

        Assert.Contains("<ellipse", svg);
    }

    [Fact]
    public void ErrorInIncludedTemplate_UnderFolderWithApostrophe_ReportsThatTemplate()
    {
        // The compiler marks every included file with a "// Codefile: <path>" comment; a path
        // containing an apostrophe used to break lexing of that marker.
        using var set = new TempIconSet();
        set.Write("o'brien/templates/t.spt", "pen.width(@@missing);\n");
        var icon = set.Write("o'brien/icon.sp", "#include \"templates/t.spt\"\n");

        var error = Assert.Throws<SyntaxErrorException>(() => Render.SvgOfFile(icon));

        Assert.EndsWith("t.spt", error.CodeFile);
        Assert.Contains("Unknown variable", error.Message);
    }

    [Fact]
    public void ErrorLineNumber_CountsCommentLines()
    {
        var error = Assert.Throws<SyntaxErrorException>(() =>
            Render.Svg("// one\n// two\npen.width(@@missing);\n"));

        Assert.StartsWith("3:", error.Statement);
    }
}
