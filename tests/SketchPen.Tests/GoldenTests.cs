using System.Runtime.CompilerServices;
using SketchPen.Tests.Support;

namespace SketchPen.Tests;

/// <summary>
/// Golden-master rendering: every Golden/&lt;name&gt;.sp is rendered to SVG and compared with the
/// checked-in Golden/&lt;name&gt;.svg. A failure means the rendered output changed -- if that was
/// intended, regenerate the expectations and review the diff:
/// <code>UPDATE_GOLDEN=1 dotnet test</code>
/// Fixtures share Golden/styles/default.globals (found through the usual "./styles" default).
/// </summary>
public class GoldenTests
{
    private static readonly string FixtureDir = Path.Combine(AppContext.BaseDirectory, "Golden");

    public static IEnumerable<object[]> Fixtures() =>
        Directory.GetFiles(FixtureDir, "*.sp")
                 .OrderBy(f => f, StringComparer.Ordinal)
                 .Select(f => new object[] { Path.GetFileNameWithoutExtension(f) });

    [Theory]
    [MemberData(nameof(Fixtures))]
    public void RendersToTheExpectedSvg(string name)
    {
        var actual = Render.Normalize(Render.SvgOfFile(Path.Combine(FixtureDir, name + ".sp")));

        if (Environment.GetEnvironmentVariable("UPDATE_GOLDEN") == "1")
        {
            File.WriteAllText(Path.Combine(SourceGoldenDir(), name + ".svg"), actual + "\n");
            return;
        }

        var expectedFile = Path.Combine(FixtureDir, name + ".svg");
        Assert.True(File.Exists(expectedFile),
            $"No expected output for '{name}'. Generate it with UPDATE_GOLDEN=1 dotnet test.");
        Assert.Equal(Render.Normalize(File.ReadAllText(expectedFile)), actual);
    }

    // The Golden folder in the source tree (not the copy next to the test binary).
    private static string SourceGoldenDir([CallerFilePath] string thisFile = "") =>
        Path.Combine(Path.GetDirectoryName(thisFile)!, "Golden");
}
