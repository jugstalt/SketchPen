using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SketchPen.Plot.Commands
{
    [PlotCommandKeyword("transform")]
    class TransformCommand : GeneralPlotCommand
    {
        public override void Execute(IPlotContext context)
        {
            switch(Method?.ToLower())
            {
                case "translate":
                    context.GraphicsContext.TranslateTransform(
                        context.Project(Parameters.Get<float>(0)),
                        context.Project(Parameters.Get<float>(1)));
                    break;
                case "rotate":
                    context.GraphicsContext.RotateTransform(Parameters.Get<float>(0));
                    break;
                case "reset":
                    context.ResetTransform();
                    break;
            }
        }
    }
}
