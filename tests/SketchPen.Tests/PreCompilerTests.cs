using System.Text.RegularExpressions;
using SketchPen.Plot.Compile;
using SketchPen.Tests.Support;

namespace SketchPen.Tests;

public class PreCompilerTests
{
    private static string PreCompile(string file, string style = "", bool appendGlobals = true) =>
        new Compiler(Render.CommandTypes).PreCompile(file, style, appendGlobals);

    private static int Count(string text, string needle) => Regex.Matches(text, Regex.Escape(needle)).Count;

    [Fact]
    public void Spaces_AreStripped_AndMissingSemicolonsAppended()
    {
        using var set = new TempIconSet();
        var file = set.Write("icon.sp", "pen.width( 6 )\r\ncircle.fill( 50 , 0 , 0 );\r\n");

        var code = PreCompile(file);

        Assert.Contains("pen.width(6);", code);
        Assert.Contains("circle.fill(50,0,0);", code);
    }

    [Fact]
    public void DefaultGlobals_AreInlinedBeforeTheFile()
    {
        using var set = new TempIconSet();
        set.Write("styles/default.globals", "globals.tryset(a, 1);\n");
        var file = set.Write("icon.sp", "circle.draw(10);\n");

        var code = PreCompile(file);

        Assert.True(code.IndexOf("globals.tryset(a,1);", StringComparison.Ordinal)
                    < code.IndexOf("circle.draw(10);", StringComparison.Ordinal));
    }

    [Fact]
    public void NamedStyle_IsInlinedBeforeDefaultGlobals()
    {
        // Order is what makes a style's `set` win over default.globals' `tryset`.
        using var set = new TempIconSet();
        set.Write("styles/default.globals", "globals.tryset(a, 1);\n");
        set.Write("styles/dark.globals", "globals.set(a, 2);\n");
        var file = set.Write("icon.sp", "circle.draw(10);\n");

        var code = PreCompile(file, style: "dark");

        var styleAt = code.IndexOf("globals.set(a,2);", StringComparison.Ordinal);
        var defaultAt = code.IndexOf("globals.tryset(a,1);", StringComparison.Ordinal);
        var fileAt = code.IndexOf("circle.draw(10);", StringComparison.Ordinal);
        Assert.True(styleAt >= 0 && styleAt < defaultAt && defaultAt < fileAt);
    }

    [Fact]
    public void AppendGlobalsFalse_LeavesDefaultGlobalsOut()
    {
        using var set = new TempIconSet();
        set.Write("styles/default.globals", "globals.tryset(a, 1);\n");
        var file = set.Write("icon.sp", "circle.draw(10);\n");

        Assert.DoesNotContain("globals.tryset", PreCompile(file, appendGlobals: false));
    }

    [Fact]
    public void Include_InlinesTheTemplate()
    {
        using var set = new TempIconSet();
        set.Write("templates/t.spt", "circle.draw(33);\n");
        var file = set.Write("icon.sp", "#include \"templates/t.spt\"\n");

        Assert.Contains("circle.draw(33);", PreCompile(file));
    }

    [Fact]
    public void MissingInclude_Throws()
    {
        using var set = new TempIconSet();
        var file = set.Write("icon.sp", "#include \"templates/nope.spt\"\n");

        var error = Assert.ThrowsAny<Exception>(() => PreCompile(file));
        Assert.Contains("Can't find include file", error.Message);
    }

    [Fact]
    public void Repeat_UnrollsTheBodyNTimes()
    {
        using var set = new TempIconSet();
        var file = set.Write("icon.sp", "repeat(4) {\n    line.draw(0, 0, 1, 1);\n}\n");

        Assert.Equal(4, Count(PreCompile(file), "line.draw(0,0,1,1);"));
    }

    [Fact]
    public void Repeat_CanBeNested()
    {
        using var set = new TempIconSet();
        var file = set.Write("icon.sp", "repeat(3) {\nrepeat(2) {\nline.draw(0, 0, 1, 1);\n}\n}\n");

        Assert.Equal(6, Count(PreCompile(file), "line.draw(0,0,1,1);"));
    }

    [Fact]
    public void Repeat_WithoutClosingBrace_Throws()
    {
        using var set = new TempIconSet();
        var file = set.Write("icon.sp", "repeat(2) {\nline.draw(0, 0, 1, 1);\n");

        var error = Assert.ThrowsAny<Exception>(() => PreCompile(file));
        Assert.Contains("Unterminated repeat", error.Message);
    }

    [Fact]
    public void Repeat_WithZeroCount_Throws()
    {
        using var set = new TempIconSet();
        var file = set.Write("icon.sp", "repeat(0) {\nline.draw(0, 0, 1, 1);\n}\n");

        var error = Assert.ThrowsAny<Exception>(() => PreCompile(file));
        Assert.Contains("positive integer", error.Message);
    }
}
