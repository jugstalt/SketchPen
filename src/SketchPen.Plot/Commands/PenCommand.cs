using SketchPen.Parse.Lexer;
using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchPen.Plot.Commands
{
    [PlotCommandKeyword("pen")]
    class PenCommand : GeneralPlotCommand
    {
        protected override void ExecuteCommand(IPlotContext context, IEnumerable<object> parameters)
        {
            switch (Method?.ToLower())
            {
                case "color":
                    context.PenColor = parameters.ToColor();
                    break;
                case "width":
                    context.PenWidth = parameters.ToTypedParameters<float>().First();
                    break;
            }
        }
    }
}
