using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SketchPen.Code.AppCode.Mvc;
using SketchPen.Code.Extensions;
using SketchPen.Code.Models.Code;
using SketchPen.Code.Services;
using SketchPen.Compose;
using SketchPen.Compose.Services.Absraction;
using SketchPen.Plot.Services;
using SketchPen.Plot.Services.Abstraction;
using SkiaSharp;
using System.Collections.Immutable;

namespace SketchPen.Code.Controllers;

[Route("[controller]")]
public class CodeController : BaseController
{
    private readonly SketchPenCodeService _sketchPenCode;
    private readonly SketchPenPlotService _sketchPenPlot;
    private readonly CompilerService _compiler;
    private readonly IEnumerable<IEditorLanguageService> _editorLanguages;
    private readonly IEnumerable<IComposerService> _composers;

    public CodeController(SketchPenCodeService sketchPenCode,
                          SketchPenPlotService sketchPenPlot,
                          CompilerService compiler,
                          IEnumerable<IEditorLanguageService> editorLanguages,
                          IEnumerable<IComposerService> composers)
    {
        _sketchPenCode = sketchPenCode;
        _sketchPenPlot = sketchPenPlot;
        _compiler = compiler;
        _editorLanguages = editorLanguages;
        _composers = composers;
    }

    [HttpGet]
    public IActionResult Index(string id)
    {
        var currentUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase.ToUriComponent()}{Request.Path}";

        return View(new IndexModel()
        {
            CurrentUrl = currentUrl,
            Id = id,
        });
    }

    [HttpGet]
    [Route("Start")]
    public IActionResult Start()
    {
        return View();
    }

    [HttpGet]
    [Route("GetFiles/{id}")]
    public IActionResult GetFiles(string id)
    {
        return base.JsonObject(_sketchPenCode.GetAllFiles(id));
    }

    #region Edit Files

    [HttpGet]
    [Route("EditFile")]
    async public Task<IActionResult> EditFile(string route)
    {
        var fileType = _sketchPenCode.GetEditorFileType(route);

        return View("EditFile", new EditFileModel()
        {
            Route = route,
            Content = await _sketchPenCode.GetFileContent(route),
            GlobalVariables = _sketchPenCode.TryGetGlobalVariableNames(route.Split('/').First()),
            EditorCompletion = _editorLanguages
                                    .Where(l => l.MatchEditorFileType(fileType))
                                    .FirstOrDefault()?
                                    .EditorCompletion
        });
    }

    [HttpPost]
    [Route("EditFile")]
    async public Task<IActionResult> EditFile(EditFileModel model)
    {
        await _sketchPenCode.SetFileContent(model.Route, model.Content);

        return Json(new { success = true });
    }

    [HttpGet]
    [Route("CreateFile/{id}")]
    async public Task<IActionResult> CreateFile(string id, string filename)
    {
        try
        {
            filename = filename.Trim();

            if (!filename.ToLower().EndsWith(".sp") &&
                !filename.ToLower().EndsWith(".globals"))
            {
                filename = $"{filename}.sp";
            }

            string content = await _sketchPenCode.CreateFile(id, filename);

            return Json(new { success = true, route = $"{id}/{filename}" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error_message = ex.Message });
        }
    }

    [HttpGet]
    [Route("DeleteFile/{id}")]
    public IActionResult RemoveFile(string id, string filename)
    {
        try
        {
            return Json(new
            {
                success = _sketchPenCode.DeleteFile(id, filename),
                route = $"{id}/{filename}"
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error_message = ex.Message });
        }
    }

    #endregion

    [HttpGet]
    [Route("GetGlobals/{id}")]
    public IActionResult GetGlobals(string id)
    {
        return base.JsonObject(_sketchPenCode.GetGlobals(id));
    }

    #region Edit Globals

    [HttpGet]
    [Route("EditGlobal")]
    public Task<IActionResult> EditGlobal(string id, string name)
    {
        if (!name.IsValidGlobalsName())
        {
            throw new Exception($"Invalid globals file name {name}");
        }

        return EditFile($"{id}/{name}");
    }

    #endregion

    #region Graphics

    [HttpGet]
    [Route("Preview")]
    public IActionResult Preview(string route, string globals, int width = 512, int height = 512, string format = "png")
    {
        var pngComposer = _composers.Where(c => c.FileExtension == "png").FirstOrDefault();
        if (pngComposer == null)
        {
            throw new Exception("Sorry, no image preview composer registered");
        }

        var fileType = _sketchPenCode.GetEditorFileType(route);
        ComposeResult composeResult;
        string contentType = "image/png";

        if (fileType == Plot.EditorFileType.Globals)
        {
            // Style/globals preview renders the whole set as one composite thumbnail
            // (see ComposeHelperService's multi-file grid layout) — vector export isn't
            // meaningful here, so this branch always stays on the PNG composer.
            string fileTitle = route.Split('/').Last(), id = route.Split('/')[0];

            composeResult = pngComposer.Compose(
                id,
                Path.Combine(_sketchPenPlot.RootPath, id),
                new[] { 64 },
                new string[] { fileTitle.Substring(1, fileTitle.Length - ".globals".Length - 1) });
        }
        else
        {
            var composer = pngComposer;

            if (string.Equals(format, "svg", StringComparison.OrdinalIgnoreCase))
            {
                composer = _composers.Where(c => c.Name == "Vector Image").FirstOrDefault();
                if (composer == null)
                {
                    throw new Exception("Sorry, no vector preview composer registered");
                }

                contentType = "image/svg+xml";
            }

            composeResult = composer.Compose(
                route.Split("/").First(),
                Path.Combine(_sketchPenPlot.RootPath, route),
                new[] { width },
                new string[] { globals });
        }

        return base.BinaryResultStream(composeResult.Data ?? Array.Empty<byte>(),
                                       contentType);
    }

    #endregion

    #region Package

    [HttpGet]
    [Route("GetComposers")]
    public IActionResult GetComposers()
    {
        return base.JsonObject(_composers.Select(c => new {
            type = c.GetType().ToString(),
            name = $"{c.Name} ({c.FileExtension.ToUpper()}-File)"
        }));
    }

    [HttpGet]
    [Route("Package")]
    async public Task<IActionResult> Package(string id, string composer, string styles, string sizes, string resolutions)
    {
        var composerInstance = _composers
            .Where(c => c.GetType().ToString().Equals(composer, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        if (composerInstance == null)
        {
            throw new Exception($"Sorry, composer with name {composer} registered");
        }

        var composeResult = composerInstance.Compose(
                id: id,
                path: Path.Combine(_sketchPenPlot.RootPath, id),
                sizes: sizes?.Split(',').Select(s => int.Parse(s)) ?? new[] { 32 },
                customGlobals: styles?.Split(',').Select(g => g.Trim().ToLower()) ?? new[] { "" },
                dpiList: resolutions?.Split(',').Select(r => (float)int.Parse(r)) ?? new[] { 96f });

        return new JsonResult(new
        {
            tempFilename = await _sketchPenPlot.WriteTempFile(composerInstance.Name.ToLower(), composerInstance.FileExtension, composeResult.Data ?? Array.Empty<byte>())
        });
    }

    [HttpGet]
    [Route("DownloadTempFile")]
    async public Task<IActionResult> DownloadTempFile(string tempFilename)
    {
        var tempFileResult = await _sketchPenPlot.ReadTempFile(tempFilename);

        return base.BinaryResultStream(tempFileResult.data ?? Array.Empty<byte>(),
                                       "application/octet-stream",
                                       tempFileResult.name);
    }

    #endregion
}
