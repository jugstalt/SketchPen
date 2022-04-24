using Microsoft.AspNetCore.Mvc;
using SketchPen.Code.AppCode.Mvc;
using SketchPen.Code.Models.Code;
using SketchPen.Code.Services;
using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Compile;

namespace SketchPen.Code.Controllers
{
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

        public IActionResult Index(string id)
        {
            var currentUrl = $"{ Request.Scheme }://{ Request.Host }{ Request.PathBase.ToUriComponent() }{ Request.Path }";

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

        public IActionResult GetFiles(string id)
        {
            return base.JsonObject(_sketchPenCode.GetAllFiles(id));
        }

        #region Edit Files

        [HttpGet]
        async public Task<IActionResult> EditFile(string route)
        {
            return View(new EditFileModel()
            {
                Route = route,
                Content = await _sketchPenCode.GetFileContent(route)
            });
        }

        [HttpPost]
        async public Task<IActionResult> EditFile(EditFileModel model)
        {
            await _sketchPenCode.SetFileContent(model.Route, model.Content);

            return Json(new { success = true });
        }

        #endregion

        #region Graphics

        public IActionResult Preview(string route, string globals, int width=512, int height=512)
        {
            var imageBytes = _sketchPenPlot.Plot(width, height, route, globals);

            return base.BinaryResultStream(imageBytes, "image/png");
        }

        #endregion
    }
}
