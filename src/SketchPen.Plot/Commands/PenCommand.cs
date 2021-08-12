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
                case "minwidth":
                    context.MinPenWidth = parameters.ToTypedParameters<float>().First();
                    break;
                case "maxwidth":
                    context.MaxPenWidth = parameters.ToTypedParameters<float>().First();
                    break;
                case "cap":
                    switch(parameters.ToTypedParameters<string>().First().ToLower().Trim())
                    {
                        case "square":
                            context.PenCap = PenCap.Square;
                            break;
                        case "flat":
                            context.PenCap = PenCap.Flat;
                            break;
                        default:
                            context.PenCap = PenCap.Round;
                            break;
                    }
                    break;
            }
        }
    }
}
