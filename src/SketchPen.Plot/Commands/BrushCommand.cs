using SketchPen.Parse.Lexer;
using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;

namespace SketchPen.Plot.Commands
{
    [PlotCommandKeyword("brush")]
    class BrushCommand : GeneralPlotCommand
    {
        public override void Execute(IPlotContext context)
        {
            switch (Method?.ToLower())
            {
                case "color":
                    context.BrushColor = Parameters.ToColor();
                    break;
            }
        }
    }
}
