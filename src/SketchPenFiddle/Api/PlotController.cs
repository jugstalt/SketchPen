using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SketchPen.Plot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SketchPenFiddle.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlotController : ControllerBase
    {
        [HttpPost]
        [Route("base64")]
        async public Task<string> Base64(int size = 100)
        {
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                string code = await reader.ReadToEndAsync();

                var plotter = new Plotter(size, size);
                var imageData = plotter.Plot(code);

                return Convert.ToBase64String(imageData);
            }
        }
    }
}
