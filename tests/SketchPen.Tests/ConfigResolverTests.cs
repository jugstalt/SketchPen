using SketchPen.Plot.Compile;
using SketchPen.Tests.Support;

namespace SketchPen.Tests;

public class ConfigResolverTests
{
    [Fact]
    public void WithoutAnyConfig_StylesDefaultToALocalStylesFolder()
    {
        using var set = new TempIconSet();

        Assert.Equal(Path.GetFullPath(set.PathOf("styles")), SketchPenConfigResolver.ResolveStylesPath(set.Root));
    }

    [Fact]
    public void StylesPath_IsResolvedRelativeToTheConfigFilesDirectory()
    {
        using var set = new TempIconSet();
        set.Write("icons/.sketchpen.json", "{ \"stylesPath\": \"../shared\" }");

        Assert.Equal(Path.GetFullPath(set.PathOf("shared")),
                     SketchPenConfigResolver.ResolveStylesPath(set.PathOf("icons")));
    }

    [Fact]
    public void FolderWithoutConfig_InheritsTheNearestAncestor()
    {
        using var set = new TempIconSet();
        set.Write("parent/.sketchpen.json", "{ \"stylesPath\": \"./styles\" }");
        Directory.CreateDirectory(set.PathOf("parent/child/grandchild"));

        // Resolved relative to where the config lives (parent), not to the folder that inherited it.
        Assert.Equal(Path.GetFullPath(set.PathOf("parent/styles")),
                     SketchPenConfigResolver.ResolveStylesPath(set.PathOf("parent/child/grandchild")));
    }

    [Fact]
    public void OwnConfig_WinsOverAnInheritedOne()
    {
        using var set = new TempIconSet();
        set.Write("parent/.sketchpen.json", "{ \"stylesPath\": \"./styles\" }");
        set.Write("parent/child/.sketchpen.json", "{ \"stylesPath\": \"./own\" }");

        Assert.Equal(Path.GetFullPath(set.PathOf("parent/child/own")),
                     SketchPenConfigResolver.ResolveStylesPath(set.PathOf("parent/child")));
    }

    [Fact]
    public void MalformedConfig_FallsThroughToTheParent()
    {
        using var set = new TempIconSet();
        set.Write("parent/.sketchpen.json", "{ \"stylesPath\": \"./styles\" }");
        set.Write("parent/child/.sketchpen.json", "{ this is not json");

        Assert.Equal(Path.GetFullPath(set.PathOf("parent/styles")),
                     SketchPenConfigResolver.ResolveStylesPath(set.PathOf("parent/child")));
    }

    [Fact]
    public void ConfigWithoutStylesPath_StillMeansALocalStylesFolder_OfTheScriptDirectory()
    {
        using var set = new TempIconSet();
        set.Write("parent/.sketchpen.json", "{ \"defaultStyle\": \"dark\" }");
        Directory.CreateDirectory(set.PathOf("parent/child"));

        Assert.Equal(Path.GetFullPath(set.PathOf("parent/child/styles")),
                     SketchPenConfigResolver.ResolveStylesPath(set.PathOf("parent/child")));
    }

    [Fact]
    public void FindConfig_ReturnsTheConfigAndTheDirectoryItWasFoundIn()
    {
        using var set = new TempIconSet();
        set.Write("parent/.sketchpen.json", "{ \"defaultStyle\": \"dark\", \"defaultSizes\": [24, 48] }");
        Directory.CreateDirectory(set.PathOf("parent/child"));

        var found = SketchPenConfigResolver.FindConfig(set.PathOf("parent/child"));

        Assert.NotNull(found);
        Assert.Equal(Path.GetFullPath(set.PathOf("parent")), found!.ConfigDirectory);
        Assert.Equal("dark", found.Config.DefaultStyle);
        Assert.Equal(new[] { 24, 48 }, found.Config.DefaultSizes);
    }

    [Fact]
    public void Keys_AreReadCaseInsensitively()
    {
        using var set = new TempIconSet();
        set.Write(".sketchpen.json", "{ \"StylesPath\": \"./x\", \"DEFAULTSTYLE\": \"dark\" }");

        var found = SketchPenConfigResolver.FindConfig(set.Root);

        Assert.Equal("./x", found!.Config.StylesPath);
        Assert.Equal("dark", found.Config.DefaultStyle);
    }
}
