using Microsoft.AspNetCore.Mvc;
using SketchPen.Code.AppCode.Mvc;
using SketchPen.Code.Models.Code;
using SketchPen.Code.Services;

namespace SketchPen.Code.Controllers
{
    [Route("[controller]")]
    public class CodeController : BaseController
    {
        private readonly SketchPenCodeService _sketchPenCode;
        private readonly SketchPenPlotService _sketchPenPlot;

        public CodeController(SketchPenCodeService sketchPenCode,
                              SketchPenPlotService sketchPenPlot)
        {
            _sketchPenCode = sketchPenCode;
            _sketchPenPlot = sketchPenPlot;
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
            return View(new EditFileModel()
            {
                Route = route,
                Content = await _sketchPenCode.GetFileContent(route)
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
            if (!filename.ToLower().EndsWith(".sp"))
            {
                filename = $"{filename}.sp";
            }

            return View("EditFile", new EditFileModel()
            {
                Route = $"{id}/{filename}",
                Content = await _sketchPenCode.CreateFile(id, filename)
            });
        }

        #endregion

        #region Graphics

        public IActionResult Preview(string route, string globals, int width = 512, int height = 512)
        {
            var imageBytes = _sketchPenPlot.Plot(width, height, route, globals);

            return base.BinaryResultStream(imageBytes, "image/png");
        }

        #endregion
    }
}
