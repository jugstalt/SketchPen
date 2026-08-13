using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SketchPen.Commands;
using SketchPen.Compose.Extensions.DependencyInjection;
using SketchPen.Extensions.DependencyInjection;
using SketchPen.Output;
using SketchPen.Plot.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.Globalization;

// Force English console/help output regardless of the host OS's UI culture, matching every
// other message this tool prints (this is what System.CommandLine's built-in --help/error text
// otherwise localizes by default).
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();

builder.Services
    .AddCompilerServices()                                   // SketchPen.Plot — reused as-is
    .AddEditorLanguagerServices()                             // SketchPen.Plot — reused as-is (powers language-info)
    .AddComposerServices<SketchPen.Plot.Skia.PlotContext>()   // SketchPen.Compose — reused as-is
    .AddSketchPenCliServices();

using var host = builder.Build();
var services = host.Services;

// ---- shared option (available on the root command and every subcommand) ----

var outputOption = new Option<string>("--output")
{
    Description = "Output mode: text (default, human-readable) or json (single structured result on stdout)",
    DefaultValueFactory = _ => "text",
    Recursive = true
};

// ---- root command: the original, legacy invocation, unchanged ----
// SketchPen.exe <path> [-outfolder|--outfolder <path>] [-custom_globals|--custom-globals <name>] [-format|--format png|svg]

var pathArgument = new Argument<string?>("path")
{
    Description = "A single .sp file, or a directory containing .sp files",
    Arity = ArgumentArity.ZeroOrOne
};

var outFolderOption = new Option<string>("--outfolder", "-outfolder")
{
    Description = "Target directory for the generated files (default: current directory)",
    DefaultValueFactory = _ => string.Empty
};

var customGlobalsOption = new Option<string>("--custom-globals", "-custom_globals")
{
    Description = "Style name — loads _<stylename>.globals in addition to _.globals",
    DefaultValueFactory = _ => string.Empty
};

var formatOption = new Option<string>("--format", "-format")
{
    Description = "png (default: 16/26/32/64/128 x @1/@2/@3) or svg (one file per icon)",
    DefaultValueFactory = _ => "png"
};

var rootSizesOption = new Option<string?>("--sizes", "-sizes")
{
    Description = "PNG only. Comma-separated sizes, e.g. 16,32,64 (default: 16,26,32,64,128)"
};

var rootResolutionsOption = new Option<string?>("--resolutions", "-resolutions")
{
    Description = "PNG only. Comma-separated @<ratio> pixel multipliers, e.g. 1,2,3 (default: 1,2,3) — " +
                   "NOT DPI (unlike compose's --resolutions)"
};

var rootDescription = """
    SketchPen — renders .sp icon scripts to PNG or SVG.

    Usage:
      SketchPen.exe <path> [-outfolder <dir>] [-custom_globals <style>] [-format png|svg]
                    [-sizes <csv>] [-resolutions <csv>]

    Examples:
      SketchPen.exe plot/basic/disk.sp -outfolder out
      SketchPen.exe plot/basic -outfolder out -custom_globals bg-dark
      SketchPen.exe plot/basic/disk.sp -outfolder out -format svg
      SketchPen.exe plot/basic -outfolder out --output json
      SketchPen.exe plot/basic/disk.sp -outfolder out -sizes 32,64 -resolutions 1,2

    -sizes/-resolutions only apply to -format png (default 16,26,32,64,128 x
    @1/@2/@3, unchanged if omitted); -format svg always renders one
    resolution-independent file per icon regardless.

    For batch ZIPs, web sprites, or the themeable CSS-variable HTML export, use
    the 'compose' subcommand instead — run 'SketchPen.exe compose --help'.
    """;

var rootCommand = new RootCommand(rootDescription)
{
    Arguments = { pathArgument },
    Options = { outFolderOption, customGlobalsOption, formatOption, rootSizesOption, rootResolutionsOption, outputOption }
};

rootCommand.SetAction(parseResult =>
{
    var reporter = CreateReporter(parseResult.GetValue(outputOption));
    var handler = services.GetRequiredService<RenderCommandHandler>();

    return handler.Execute(
        parseResult.GetValue(pathArgument),
        parseResult.GetValue(outFolderOption) ?? string.Empty,
        parseResult.GetValue(customGlobalsOption) ?? string.Empty,
        parseResult.GetValue(formatOption) ?? "png",
        parseResult.GetValue(rootSizesOption),
        parseResult.GetValue(rootResolutionsOption),
        reporter);
});

// ---- compose subcommand: invoke any registered IComposerService directly ----
// SketchPen.exe compose <path> --composer <id> [--sizes ...] [--styles ...] [--resolutions ...] [--out <file>]

var composePathArgument = new Argument<string?>("path")
{
    Description = "A single .sp file, or a directory containing .sp files",
    Arity = ArgumentArity.ZeroOrOne
};

var composerOption = new Option<string?>("--composer")
{
    Description = "Composer id — png, png-zip, web-sprite-zip, svg, svg-zip, svg-vars-zip"
};

var sizesOption = new Option<string?>("--sizes")
{
    Description = "Comma-separated sizes, e.g. 16,32,64"
};

var stylesOption = new Option<string?>("--styles")
{
    Description = "Comma-separated style names, empty entry = default style, e.g. ,bg-dark"
};

var resolutionsOption = new Option<string?>("--resolutions")
{
    Description = "Comma-separated DPI values, e.g. 96,144,192"
};

var outOption = new Option<string?>("--out")
{
    Description = "Output file path (default: <name>.<composer-extension> in the current directory)"
};

// Short — this exact string is what shows up in the ROOT command's "Commands:" list
// (System.CommandLine reuses Description for both purposes), so it must stay to the point.
// The full walkthrough below is appended only when 'compose --help' is run directly (see
// AppendExtraHelp below), so it never clutters "SketchPen.exe --help".
var composeDescription =
    "Render icons via a registered composer (PNG/SVG/ZIP/HTML export) — run 'SketchPen.exe compose --help' for composer ids and examples.";

var composeExtraHelp = """

    IMPORTANT: <path> goes right after 'compose', not before it, e.g.
    'SketchPen.exe compose plot/webgis --composer svg-zip' — NOT
    'SketchPen.exe plot/webgis compose ...'.

    Composer ids:
      png             single PNG (needs exactly one --sizes value)
      png-zip         ZIP with all icons, split by style/size/resolution
      web-sprite-zip  ZIP with a CSS sprite sheet + demo HTML page
      svg             single SVG (needs exactly one --sizes value)
      svg-zip         ZIP with all icons as individual SVGs
      svg-vars-zip    ZIP with themeable CSS-variable HTML pages, one per icon

    Examples:
      SketchPen.exe compose plot/basic/disk.sp --composer svg --sizes 64 --out disk.svg
      SketchPen.exe compose plot/webgis --composer svg-zip --styles ,bg-dark --out webgis-svg.zip
      SketchPen.exe compose plot/basic --composer svg-vars-zip --out basic-themeable.zip
      SketchPen.exe compose plot/basic --composer web-sprite-zip --sizes 16,32,64 --out basic-sprites.zip
      SketchPen.exe compose plot/basic/disk.sp --composer png --sizes 128 --output json

    See docs/CLI.md for the full composer table and JSON output shape.
    """;

var composeCommand = new Command("compose", composeDescription)
{
    Arguments = { composePathArgument },
    Options = { composerOption, sizesOption, stylesOption, resolutionsOption, outOption }
};

// Append the full walkthrough (composer ids + examples) after the auto-generated help, but
// only for 'compose --help' itself — the short Description above is what appears in the root
// command's "Commands:" list. Explicitly adding our own HelpOption (rather than searching for
// one on composeCommand.Options) is necessary because a subcommand's default HelpOption isn't
// present on .Options until the tree is built for invocation — too late to wrap here.
var composeHelpOption = new HelpOption();
composeHelpOption.Action = new AppendingHelpAction((HelpAction)composeHelpOption.Action!, composeExtraHelp);
composeCommand.Options.Add(composeHelpOption);

composeCommand.SetAction(parseResult =>
{
    var reporter = CreateReporter(parseResult.GetValue(outputOption));
    var handler = services.GetRequiredService<ComposeCommandHandler>();

    return handler.Execute(
        parseResult.GetValue(composePathArgument),
        parseResult.GetValue(composerOption),
        parseResult.GetValue(sizesOption),
        parseResult.GetValue(stylesOption),
        parseResult.GetValue(resolutionsOption),
        parseResult.GetValue(outOption),
        reporter);
});

rootCommand.Subcommands.Add(composeCommand);

// ---- language-info subcommand: machine-readable completion grammar + per-project globals ----
// SketchPen.exe language-info [path]
// Always emits one JSON document, no --output option -- this is a tooling-only command (e.g.
// for the VS Code extension's completion provider), not meant for direct human reading.

var languageInfoPathArgument = new Argument<string?>("path")
{
    Description = "Optional: a directory (or a .sp file inside one) to also resolve @@variable names from its _.globals",
    Arity = ArgumentArity.ZeroOrOne
};

var languageInfoCommand = new Command("language-info",
    "Print the completion grammar (and, given a path, that project's @@variable names) as JSON. For tooling, not interactive use.")
{
    Arguments = { languageInfoPathArgument }
};

languageInfoCommand.SetAction(parseResult =>
{
    var handler = services.GetRequiredService<LanguageInfoCommandHandler>();

    return handler.Execute(parseResult.GetValue(languageInfoPathArgument));
});

rootCommand.Subcommands.Add(languageInfoCommand);

return rootCommand.Parse(args).Invoke();

static IConsoleReporter CreateReporter(string? mode) =>
    string.Equals(mode, "json", System.StringComparison.OrdinalIgnoreCase)
        ? new JsonConsoleReporter()
        : new TextConsoleReporter();

// Wraps a command's default --help action to print extra content (composer ids, worked
// examples) after the auto-generated Description/Usage/Options/Arguments sections, without
// that extra content becoming part of Description itself (which is also what a parent
// RootCommand shows for this command in its own "Commands:" list — keeping Description short
// there while still giving a rich `compose --help` is the whole point of this wrapper).
sealed class AppendingHelpAction : SynchronousCommandLineAction
{
    private readonly HelpAction _defaultHelp;
    private readonly string _extra;

    public AppendingHelpAction(HelpAction defaultHelp, string extra)
    {
        _defaultHelp = defaultHelp;
        _extra = extra;
    }

    public override int Invoke(ParseResult parseResult)
    {
        int result = _defaultHelp.Invoke(parseResult);
        Console.WriteLine(_extra);
        return result;
    }
}
