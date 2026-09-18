using Microsoft.Extensions.DependencyInjection;
using SketchPen.Commands;
using SketchPen.Compose.Extensions.DependencyInjection;
using SketchPen.Extensions.DependencyInjection;
using SketchPen.Plot.Extensions.DependencyInjection;
using SketchPen.Plot.Skia;
using SketchPen.Tests.Support;

namespace SketchPen.Tests;

/// <summary>
/// The two CLI command handlers (root command and `compose`) with .sketchpen.json in play:
/// config defaults, explicit-flag precedence, named export profiles.
/// </summary>
public class CommandHandlerTests
{
    private static readonly ServiceProvider Services = new ServiceCollection()
        .AddCompilerServices()
        .AddEditorLanguagerServices()
        .AddComposerServices<PlotContext>()
        .AddSketchPenCliServices()
        .BuildServiceProvider();

    private static readonly byte[] PngMagic = { 0x89, 0x50, 0x4E, 0x47 };

    private static string RenderSvg(string spFile, string style = "")
    {
        using var set = new TempIconSet(); // only for a scratch output folder
        var reporter = new RecordingReporter();
        var code = new RenderCommandHandler(Render.CommandTypes)
            .Execute(spFile, set.PathOf("out"), style, "svg", null, null, reporter);

        Assert.True(code == 0, string.Join("; ", reporter.Errors));
        return File.ReadAllText(Directory.GetFiles(set.PathOf("out"), "*.svg").Single());
    }

    // ---- root command: .sketchpen.json defaults --------------------------------------------

    [Fact]
    public void ConfigDefaults_SizesAndResolutions_AreUsedWhenNoFlagsAreGiven()
    {
        using var set = new TempIconSet();
        set.Write(".sketchpen.json", "{ \"defaultSizes\": [24, 48], \"defaultResolutions\": [1, 2] }");
        var icon = set.Write("icon.sp", "circle.draw(40);\n");
        var reporter = new RecordingReporter();

        var code = new RenderCommandHandler(Render.CommandTypes)
            .Execute(icon, set.PathOf("out"), "", "png", null, null, reporter);

        Assert.Equal(0, code);
        Assert.Equal(new[] { "icon_24@1.png", "icon_24@2.png", "icon_48@1.png", "icon_48@2.png" },
                     Directory.GetFiles(set.PathOf("out")).Select(Path.GetFileName).Order().ToArray());
    }

    [Fact]
    public void ExplicitFlags_WinOverConfigDefaults()
    {
        using var set = new TempIconSet();
        set.Write(".sketchpen.json", "{ \"defaultSizes\": [24, 48], \"defaultResolutions\": [1, 2] }");
        var icon = set.Write("icon.sp", "circle.draw(40);\n");

        var code = new RenderCommandHandler(Render.CommandTypes)
            .Execute(icon, set.PathOf("out"), "", "png", "100", "1", new RecordingReporter());

        Assert.Equal(0, code);
        Assert.Equal(new[] { "icon_100@1.png" },
                     Directory.GetFiles(set.PathOf("out")).Select(Path.GetFileName).ToArray());
    }

    [Fact]
    public void NoConfig_UsesTheBuiltInSizeMatrix()
    {
        using var set = new TempIconSet();
        var icon = set.Write("icon.sp", "circle.draw(40);\n");

        new RenderCommandHandler(Render.CommandTypes)
            .Execute(icon, set.PathOf("out"), "", "png", null, null, new RecordingReporter());

        Assert.Equal(15, Directory.GetFiles(set.PathOf("out")).Length); // 5 sizes x 3 resolutions
    }

    [Fact]
    public void ConfigDefaultStyle_IsApplied_AndAnExplicitStyleWinsOverIt()
    {
        using var set = new TempIconSet();
        set.Write(".sketchpen.json", "{ \"defaultStyle\": \"dark\" }");
        set.Write("styles/default.globals", "globals.tryset(c, \"#000000\");\npen.color(@@c);\npen.width(2);\n");
        set.Write("styles/dark.globals", "globals.set(c, \"#ffffff\");\n");
        set.Write("styles/light.globals", "globals.set(c, \"#ff0000\");\n");
        var icon = set.Write("icon.sp", "circle.draw(40);\n");

        Assert.Contains("stroke=\"white\"", RenderSvg(icon));                 // defaultStyle: dark
        Assert.Contains("stroke=\"red\"", RenderSvg(icon, style: "light"));   // explicit -style wins
    }

    [Fact]
    public void ConfigOutFolder_IsRelativeToTheConfigFile_NotTheWorkingDirectory()
    {
        using var set = new TempIconSet();
        set.Write(".sketchpen.json", "{ \"outFolder\": \"./rendered\" }");
        var icon = set.Write("sub/icon.sp", "circle.draw(40);\n");
        // The config sits one level above the icon: the inherited outFolder resolves next to it.

        var code = new RenderCommandHandler(Render.CommandTypes)
            .Execute(icon, "", "", "png", "16", "1", new RecordingReporter());

        Assert.Equal(0, code);
        Assert.True(File.Exists(set.PathOf("rendered/icon_16@1.png")));
    }

    // ---- compose --profile -----------------------------------------------------------------

    [Fact]
    public void ComposeProfile_SuppliesComposerAndSizes()
    {
        using var set = new TempIconSet();
        set.Write(".sketchpen.json",
            "{ \"exportProfiles\": { \"small\": { \"composer\": \"svg\", \"sizes\": \"64\" } } }");
        var icon = set.Write("icon.sp", "circle.draw(40);\n");
        var reporter = new RecordingReporter();

        var code = Services.GetRequiredService<ComposeCommandHandler>()
            .Execute(icon, null, null, null, null, set.PathOf("out.svg"), "small", reporter);

        Assert.True(code == 0, string.Join("; ", reporter.Errors));
        Assert.Contains("width=\"64\"", File.ReadAllText(set.PathOf("out.svg")));
    }

    [Fact]
    public void ComposeProfile_ExplicitComposerWinsOverTheProfilesOne()
    {
        using var set = new TempIconSet();
        set.Write(".sketchpen.json",
            "{ \"exportProfiles\": { \"small\": { \"composer\": \"svg\", \"sizes\": \"64\" } } }");
        var icon = set.Write("icon.sp", "circle.draw(40);\n");

        var code = Services.GetRequiredService<ComposeCommandHandler>()
            .Execute(icon, "png", null, null, null, set.PathOf("out.png"), "small", new RecordingReporter());

        Assert.Equal(0, code);
        Assert.Equal(PngMagic, File.ReadAllBytes(set.PathOf("out.png")).Take(4).ToArray());
    }

    [Fact]
    public void ComposeProfile_Unknown_FailsWithAClearMessage()
    {
        using var set = new TempIconSet();
        var icon = set.Write("icon.sp", "circle.draw(40);\n");
        var reporter = new RecordingReporter();

        var code = Services.GetRequiredService<ComposeCommandHandler>()
            .Execute(icon, null, null, null, null, set.PathOf("out.svg"), "nope", reporter);

        Assert.Equal(1, code);
        Assert.Contains(reporter.Errors, e => e.Contains("Unknown --profile 'nope'"));
    }

    [Fact]
    public void Compose_WithoutComposerOrProfile_FailsWithAClearMessage()
    {
        using var set = new TempIconSet();
        var icon = set.Write("icon.sp", "circle.draw(40);\n");
        var reporter = new RecordingReporter();

        var code = Services.GetRequiredService<ComposeCommandHandler>()
            .Execute(icon, null, null, null, null, set.PathOf("out.svg"), null, reporter);

        Assert.Equal(1, code);
        Assert.Contains(reporter.Errors, e => e.Contains("--composer"));
    }
}
