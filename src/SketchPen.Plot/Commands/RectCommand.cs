using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SketchPen.Plot.Commands;

[PlotCommandKeyword("rect")]
[PlotCommandSupportedFileTypes(EditorFileType.Template | EditorFileType.Code)]
[PlotCommandMethod("draw", suggestion: "draw(width, height)", snippet: "draw(${1:width}, ${2:height});")]
[PlotCommandMethod("draw", suggestion: "draw(width, height, cornerRadius)", snippet: "draw(${1:width}, ${2:height}, ${3:cornerRadius});")]
[PlotCommandMethod("draw", suggestion: "draw(width, height, cornerRadius, color)", snippet: "draw(${1:width}, ${2:height}, ${3:cornerRadius}, \"${4:hexColor}\");")]

[PlotCommandMethod("fill", suggestion: "fill(width, height)", snippet: "fill(${1:width}, ${2:height});")]
[PlotCommandMethod("fill", suggestion: "fill(width, height, cornerRadius)", snippet: "fill(${1:width}, ${2:height}, ${3:cornerRadius});")]
[PlotCommandMethod("fill", suggestion: "fill(width, height, cornerRadius, color)", snippet: "fill(${1:width}, ${2:height}, ${3:cornerRadius}, \"${4:hexColor}\");")]
class RectCommand : GeneralPlotCommand
{
    protected override void ExecuteCommand(IPlotContext context, IEnumerable<object> parameters)
    {
        var shape = parameters.TakeFirstTypedParameterBlock<float>();
        if (shape.Length < 2)
        {
            throw new Exception("rect.draw/fill require at least width and height");
        }

        float width = shape[0];
        float height = shape[1];
        float cornerRadius = shape.Length > 2 ? shape[2] : 0f;

        var rect = new CanvasRectangle(-width / 2f, -height / 2f, width, height);
        var tail = parameters.Skip(shape.Length);

        switch (Method?.ToLower())
        {
            case "draw":
                using (var pen = context.CreatePen(tail))
                {
                    context.Canvas.DrawRect(pen, context.Project(rect), context.Project(cornerRadius));
                }
                break;
            case "fill":
                using (var brush = context.CreateBrush(tail))
                {
                    context.Canvas.FillRect(brush, context.Project(rect), context.Project(cornerRadius));
                }
                break;
            default:
                throw new Exception($"Unknown method: {Method}");
        }
    }
}
