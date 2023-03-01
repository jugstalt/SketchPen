using Microsoft.AspNetCore.Mvc;
using SketchPen.Code.AppCode.Mvc;
using SketchPen.Code.Extensions;
using SketchPen.Code.Models.Code;
using SketchPen.Code.Services;
using SketchPen.Plot.Services;
using SketchPen.Plot.Services.Abstraction;

namespace SketchPen.Code.Controllers;

[Route("[controller]")]
public class CodeController : BaseController
{
    private readonly SketchPenCodeService _sketchPenCode;
    private readonly SketchPenPlotService _sketchPenPlot;
    private readonly CompilerService _compiler;
    private readonly IEnumerable<IEditorLanguageService> _editorLanguages;

    public CodeController(SketchPenCodeService sketchPenCode,
                          SketchPenPlotService sketchPenPlot,
                          CompilerService compiler,
                          IEnumerable<IEditorLanguageService> editorLanguages)
    {
        _sketchPenCode = sketchPenCode;
        _sketchPenPlot = sketchPenPlot;
        _compiler = compiler;
        _editorLanguages = editorLanguages;
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

        var variables = await _sketchPenCode.GetGlobalVariableNames(route.Split('/').First());

        return View("EditFile", new EditFileModel()
        {
            Route = route,
            Content = await _sketchPenCode.GetFileContent(route),
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
        var imageBytes = _sketchPenPlot.Plot(width, height, route, globals);

        return base.BinaryResultStream(imageBytes, "image/png");
    }

    #endregion
}
