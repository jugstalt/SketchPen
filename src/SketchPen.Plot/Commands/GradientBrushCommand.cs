using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchPen.Plot.Commands
{
    [PlotCommandKeyword("gradientbrush")]
    class GradientBrushCommand : GeneralPlotCommand
    {
        public override void Execute(IPlotContext context)
        {
            switch(Method?.ToLower())
            {
                case "color":
                    context.GradientBrushColor = Parameters.ToColor();
                    break;
                case "points":
                    context.GradientBrushPoint1 = Parameters.ToPoints().FirstOrDefault();
                    context.GradientBrushPoint2 = Parameters.ToPoints().Skip(1).FirstOrDefault();
                    break;
            }
        }
    }
}
