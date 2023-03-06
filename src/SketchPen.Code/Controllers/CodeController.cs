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
        return Index(String.Empty);
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
            GlobalVariables = await _sketchPenCode.TryGetGlobalVariableNames(route.Split('/').First()),
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
    public IActionResult Preview(string route, string globals, int width = 512, int height = 512)
    {
        var composer = _composers.Where(c => c.ContentType == "image/png").FirstOrDefault();
        if (composer == null)
        {
            throw new Exception("Sorry, no image preview composer registered");
        }

        var fileType = _sketchPenCode.GetEditorFileType(route);
        ComposeResult composeResult;

        if (fileType == Plot.EditorFileType.Globals)
        {
            string fileTitle = route.Split('/').Last();

            composeResult = composer.Compose(
                Path.Combine(_sketchPenPlot.RootPath, route.Split('/')[0]),
                new[] { 64 },
                new string[] { fileTitle.Substring(1, fileTitle.Length - ".globals".Length - 1) });
        }
        else
        {
            composeResult = composer.Compose(
                Path.Combine(_sketchPenPlot.RootPath, route),
                new[] { width },
                new string[] { globals });
        }

        return base.BinaryResultStream(composeResult.Data ?? Array.Empty<byte>(), 
                                       composer.ContentType);
    }

    #endregion

    #region Package

    [HttpGet]
    [Route("GetComposers")]
    public IActionResult GetComposers()
    {
        return base.JsonObject(_composers.Select(c => c.Name));
    }

    [HttpGet]
    [Route("Package")]
    public IActionResult Package(string id, string globals, string sizes)
    {
        var composer = _composers.Where(c => c.ContentType == "application/zip").FirstOrDefault();
        if (composer == null)
        {
            throw new Exception("Sorry, no image package composer registered");
        }

        var composeResult = composer.Compose(
                Path.Combine(_sketchPenPlot.RootPath, id),
                sizes.Split(',').Select(s => int.Parse(s)),
                globals.Split(',').Select(g => g.Trim().ToLower()),
                new[] { 96f, 144f, 192f });

        return base.BinaryResultStream(composeResult.Data ?? Array.Empty<byte>(),
                                       composer.ContentType);
    }

    #endregion
}
