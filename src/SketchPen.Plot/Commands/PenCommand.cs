using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SketchPen.Plot.Commands;

[PlotCommandKeyword("pen")]
[PlotCommandSupportedFileTypes(EditorFileType.Globals | EditorFileType.Template | EditorFileType.Code)]
[PlotCommandMethod("color", snippet: "color(\"${1:hexColor}\");")]
[PlotCommandMethod("width", snippet: "width(${1:width});")]
[PlotCommandMethod("minwidth", snippet: "minwidth(${1:minWidth});")]
[PlotCommandMethod("maxwidth", snippet: "maxwidth(${1:maxWidth});")]
[PlotCommandMethod("cap", snippet: "cap(\"${1:round|flat|square}\");")]
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
                switch (parameters.ToTypedParameters<string>().First().ToLower().Trim())
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
            default:
                throw new Exception($"Unknown method: {Method}");
        }
    }
}
