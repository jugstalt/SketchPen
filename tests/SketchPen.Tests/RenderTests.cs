using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using SketchPen.Plot;
using SketchPen.Plot.Exceptions;
using SketchPen.Plot.Skia;
using SketchPen.Tests.Support;

namespace SketchPen.Tests;

/// <summary>
/// Regression tests for behavior that broke once (each names the bug it guards) plus the
/// language features whose semantics are easy to get subtly wrong.
/// </summary>
public class RenderTests
{
    private const string Pen = "pen.color(\"#000\");\npen.width(2);\n";

    // ---- circle: a trailing color must survive a short position block ---------------------

    [Fact]
    public void Circle_InlineFillColor_IsKept_WhenPositionIsJustADiameter()
    {
        // Used to be dropped: the color tail was cut at a fixed offset of 4 instead of after
        // however many position values were actually given.
        var svg = Render.Svg("circle.fill(70, \"#4a90d9\");\n");

        Assert.Contains("fill=\"#4A90D9\"", svg);
    }

    [Fact]
    public void Circle_InlineDrawColor_IsKept_WithThreePositionValues()
    {
        var svg = Render.Svg("pen.width(4);\ncircle.draw(50, 10, 10, \"#ff0000\");\n");

        Assert.Contains("stroke=\"red\"", svg);
    }

    // ---- gradientbrush.points --------------------------------------------------------------

    private const string GradientScript =
        "brush.color(\"#fff\");\ngradientbrush.color(\"#000\");\ngradientbrush.points(-10, -20, 30, 40);\ncircle.fill(50);\n";

    private static double[] GradientAxis(string svg)
    {
        var match = Regex.Match(svg, "x1=\"([-\\d.]+)\" y1=\"([-\\d.]+)\" x2=\"([-\\d.]+)\" y2=\"([-\\d.]+)\"");
        Assert.True(match.Success, "no linear gradient in the SVG");
        return match.Groups.Cast<Group>().Skip(1)
                    .Select(g => double.Parse(g.Value, CultureInfo.InvariantCulture)).ToArray();
    }

    [Fact]
    public void GradientBrushPoints_UseBothGivenPoints_InLogicalUnits()
    {
        // At 128 px one logical unit is 1.28 px. The second point used to be reset to (0,0)
        // whatever was passed, and both were taken as raw pixels.
        var axis = GradientAxis(Render.Svg(GradientScript, size: 128));

        Assert.Equal(-12.8, axis[0], 1);
        Assert.Equal(-25.6, axis[1], 1);
        Assert.Equal(38.4, axis[2], 1);
        Assert.Equal(51.2, axis[3], 1);
    }

    [Fact]
    public void GradientBrushPoints_ScaleWithTheRenderSize()
    {
        var small = GradientAxis(Render.Svg(GradientScript, size: 64));
        var large = GradientAxis(Render.Svg(GradientScript, size: 256));

        for (var i = 0; i < 4; i++)
        {
            Assert.Equal(large[i] / 256, small[i] / 64, 3); // same position relative to the canvas
        }
    }

    // ---- state must not leak between renders of the same compiled script -------------------

    [Fact]
    public void ImplicitPath_DoesNotLeakBetweenRendersOfTheSameScript()
    {
        // The classic CLI compiles a script once and plots it once per size. A path built without
        // an explicit path.begin() used to keep accumulating in a static field across those plots.
        using var set = new TempIconSet();
        var file = set.Write("icon.sp", Pen + "path.start();\npath.addlines(-20, -20, 20, 20);\npath.draw();\n");
        var plotter = new Plotter(Render.CommandTypes, typeof(SvgPlotContext));
        plotter.Init(file);

        var first = Encoding.UTF8.GetString(plotter.Plot(64, 64, EncodeFormat.Svg));
        var second = Encoding.UTF8.GetString(plotter.Plot(64, 64, EncodeFormat.Svg));

        Assert.Equal(first, second);
    }

    // ---- variables and arithmetic ---------------------------------------------------------

    [Theory]
    [InlineData("circle.draw(10*2);", "circle.draw(20);")]
    [InlineData("circle.draw(10+5*2);", "circle.draw(20);")]
    [InlineData("circle.draw((10+5)*2);", "circle.draw(30);")]
    [InlineData("circle.draw(@@x-10);", "circle.draw(30);")]
    [InlineData("circle.draw(@@x - 10);", "circle.draw(30);")]
    [InlineData("circle.draw(@@x/4);", "circle.draw(10);")]
    public void Arithmetic_EqualsTheWrittenOutLiteral(string expression, string literal)
    {
        const string globals = "globals.tryset(x, 40);\n";

        Assert.Equal(Render.Svg(Pen + literal, globals), Render.Svg(Pen + expression, globals));
    }

    [Fact]
    public void Variables_ComeFromDefaultGlobals_AndANamedStyleOverridesThem()
    {
        const string globals = "globals.tryset(c, \"#ff0000\");\npen.color(@@c);\npen.width(2);\n";
        var styles = new Dictionary<string, string> { ["styles/blue.globals"] = "globals.set(c, \"#0000ff\");\n" };

        var plain = Render.Svg("circle.draw(40);\n", globals);
        var styled = Render.Svg("circle.draw(40);\n", globals, style: "blue", extraFiles: styles);

        Assert.Contains("stroke=\"red\"", plain);
        Assert.Contains("stroke=\"blue\"", styled);
    }

    [Fact]
    public void UnknownVariable_ReportsASyntaxError()
    {
        var error = Assert.Throws<SyntaxErrorException>(() => Render.Svg("pen.width(@@nope);\n"));

        Assert.Contains("Unknown variable", error.Message);
    }

    // ---- repeat / include -----------------------------------------------------------------

    [Fact]
    public void Repeat_RendersTheSameAsTheUnrolledStatements()
    {
        var repeated = Render.Svg(Pen + "repeat(3) {\ntransform.rotate(30);\nline.draw(0, 0, 0, -20);\n}\n");
        var unrolled = Render.Svg(Pen +
            "transform.rotate(30);\nline.draw(0, 0, 0, -20);\n" +
            "transform.rotate(30);\nline.draw(0, 0, 0, -20);\n" +
            "transform.rotate(30);\nline.draw(0, 0, 0, -20);\n");

        Assert.Equal(unrolled, repeated);
    }

    [Fact]
    public void Include_RendersTheTemplate()
    {
        var svg = Render.Svg(Pen + "#include \"templates/ring.spt\"\n",
            extraFiles: new Dictionary<string, string> { ["templates/ring.spt"] = "circle.draw(30);\n" });

        Assert.Contains("<ellipse", svg);
    }

    // ---- misc -------------------------------------------------------------------------------

    [Fact]
    public void Svg_ScalesWithTheRequestedSize()
    {
        var small = Render.Svg(Pen + "circle.draw(50);\n", size: 64);
        var large = Render.Svg(Pen + "circle.draw(50);\n", size: 256);

        Assert.Contains("width=\"64\"", small);
        Assert.Contains("width=\"256\"", large);
    }

    [Fact]
    public void UnknownKeyword_ReportsASyntaxError()
    {
        Assert.Throws<SyntaxErrorException>(() => Render.Svg("blorp.draw(10);\n"));
    }
}
