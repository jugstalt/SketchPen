using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchPen.Plot.Commands;

[PlotCommandKeyword("text")]
[PlotCommandSupportedFileTypes(EditorFileType.Template | EditorFileType.Code)]
[PlotCommandMethod("draw", snippet: "draw(${1:text},${2:size},${3:point_x}, ${4:point_y});")]
class TextCommand : GeneralPlotCommand
{
    protected override void ExecuteCommand(IPlotContext context, IEnumerable<object> parameters)
    {
        switch (Method?.ToLower())
        {
            case "draw":
                var text = parameters.ElementAtOrDefault(0)?.ToString() ?? string.Empty;
                var size = parameters.Get<float>(1);
                var point = parameters.Skip(2).ToPoints().FirstOrDefault();
                if (point != null)
                {
                    using (var brush = context.CreateBrush(parameters.Skip(4)))
                    using (var font = context.CreateFont([size]))
                    {
                        context.Canvas.DrawText(font, text, context.Project(point), brush);
                    }
                }
                break;
            default:
                throw new Exception($"Unknown method: {Method}");
        }
    }
}
