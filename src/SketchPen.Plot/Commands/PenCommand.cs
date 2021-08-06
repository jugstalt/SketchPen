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
        public override void Execute(IPlotContext context)
        {
            switch (Method?.ToLower())
            {
                case "color":
                    context.PenColor = Parameters.ToColor();
                    break;
                case "width":
                    context.PenWidth = Parameters.ToTypedParameters<float>().First();
                    break;
            }
        }
    }
}
