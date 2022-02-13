using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SketchPen.Plot.Commands
{
    [PlotCommandKeyword("transform")]
    class TransformCommand : GeneralPlotCommand
    {
        protected override void ExecuteCommand(IPlotContext context, IEnumerable<object> parameters)
        {
            switch (Method?.ToLower())
            {
                case "translate":
                    context.Canvas.TranslateTransform(
                        context.Project(parameters.Get<float>(0)),
                        context.Project(parameters.Get<float>(1)));
                    break;
                case "rotate":
                    context.Canvas.RotateTransform(parameters.Get<float>(0));
                    break;
                case "scale":
                    context.Canvas.ScaleTransform(
                        parameters.Get<float>(0),
                        parameters.Get<float>(parameters.Count() > 1 ? 1 : 0));
                    break;
                case "reset":
                    context.ResetTransform();
                    break;
                default:
                    throw new Exception($"Unknown method: { Method  }");
            }
        }
    }
}
