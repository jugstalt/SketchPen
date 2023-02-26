using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SketchPen.Plot.Commands
{
    [PlotCommandKeyword("circle")]
    [PlotCommandMethod("draw", label: "draw(diameter)", insertText: "draw(${1:diameter});")]
    [PlotCommandMethod("draw", label: "draw(diameterX, diameterY)", insertText: "draw(${1:diameterX}, ${2:diameterY});")]
    [PlotCommandMethod("draw", label: "draw(diameter, centerX, centerY)", insertText: "draw(${1:diameter}, ${2:centerX}, ${3:centerY});")]
    [PlotCommandMethod("draw", label: "draw(diameterX, diameterY, centerX, centerY)", insertText: "draw(${1:diameterX}, ${2:diameterY}, ${3:centerX}, ${4:centerY});")]
    [PlotCommandMethod("draw", label: "draw(diameterX, diameterY, centerX, centerY, color)", insertText: "draw(${1:diameterX}, ${2:diameterY}, ${3:centerX}, ${4:centerY}, \"${5:hexColor}\");")]

    [PlotCommandMethod("fill", label: "fill(diameter)", insertText: "fill(${1:diameter});")]
    [PlotCommandMethod("fill", label: "fill(diameterX, diameterY)", insertText: "fill(${1:diameterX}, ${2:diameterY});")]
    [PlotCommandMethod("fill", label: "fill(diameter, centerX, centerY)", insertText: "fill(${1:diameter}, ${2:centerX}, ${3:centerY});")]
    [PlotCommandMethod("fill", label: "fill(diameterX, diameterY, centerX, centerY)", insertText: "fill(${1:diameterX}, ${2:diameterY}, ${3:centerX}, ${4:centerY});")]
    [PlotCommandMethod("fill", label: "fill(diameterX, diameterY, centerX, centerY, color)", insertText: "fill(${1:diameterX}, ${2:diameterY}, ${3:centerX}, ${4:centerY}, \"${5:hexColor}\");")]

    [PlotCommandMethod("arc", label: "arc(startAngle, sweepAngle, diameter)", insertText: "arc(${1:startAngle}, ${2:sweepAngle}, ${3:diameter});")]
    [PlotCommandMethod("arc", label: "arc(startAngle, sweepAngle, diameterX, diameterY)", insertText: "arc(${1:startAngle}, ${2:sweepAngle}, ${3:diameterX}, ${4:diameterY});")]
    [PlotCommandMethod("arc", label: "arc(startAngle, sweepAngle, diameter, centerX, centerY)", insertText: "arc(${1:startAngle}, ${2:sweepAngle}, ${3:diameter}, ${4:centerX}, ${5:centerY});")]
    [PlotCommandMethod("arc", label: "arc(startAngle, sweepAngle, diameterX, diameterY, centerX, centerY)", insertText: "arc(${1:startAngle}, ${2:sweepAngle}, ${3:diameterX}, ${4:diameterY}, ${5:centerX}, ${6:centerY});")]
    [PlotCommandMethod("arc", label: "arc(startAngle, sweepAngle, diameterX, diameterY, centerX, centerY, color)", insertText: "arc(${1:startAngle}, ${2:sweepAngle}, ${3:diameterX}, ${4:diameterY}, ${5:centerX}, ${6:centerY}, \"${7:hexColor}\");")]

    [PlotCommandMethod("pie", label: "pie(startAngle, sweepAngle, diameter)", insertText: "pie(${1:startAngle}, ${2:sweepAngle}, ${3:diameter});")]
    [PlotCommandMethod("pie", label: "pie(startAngle, sweepAngle, diameterX, diameterY)", insertText: "pie(${1:startAngle}, ${2:sweepAngle}, ${3:diameterX}, ${4:diameterY});")]
    [PlotCommandMethod("pie", label: "pie(startAngle, sweepAngle, diameter, centerX, centerY)", insertText: "pie(${1:startAngle}, ${2:sweepAngle}, ${3:diameter}, ${4:centerX}, ${5:centerY});")]
    [PlotCommandMethod("pie", label: "pie(startAngle, sweepAngle, diameterX, diameterY, centerX, centerY)", insertText: "pie(${1:startAngle}, ${2:sweepAngle}, ${3:diameterX}, ${4:diameterY}, ${5:centerX}, ${6:centerY});")]
    [PlotCommandMethod("pie", label: "pie(startAngle, sweepAngle, diameterX, diameterY, centerX, centerY, color)", insertText: "pie(${1:startAngle}, ${2:sweepAngle}, ${3:diameterX}, ${4:diameterY}, ${5:centerX}, ${6:centerY}, \"${7:hexColor}\");")]

    class CircleCommand : GeneralPlotCommand
    {
        protected override void ExecuteCommand(IPlotContext context, IEnumerable<object> parameters)
        {
            CanvasRectangle pos;

            switch (Method?.ToLower())
            {
                case "arc":
                case "pie":
                    pos = parameters.Skip(2).ToRectPos();
                    break;
                default:
                    pos = parameters.ToRectPos();
                    break;
            }

            switch (Method?.ToLower())
            {
                case "draw":
                    using (var pen = context.CreatePen(parameters.Skip(4)))
                    {
                        context.Canvas.DrawEllipse(pen, context.Project(pos));
                    }
                    break;
                case "fill":
                    using (var brush = context.CreateBrush(parameters.Skip(4)))
                    {
                        context.Canvas.FillEllipse(brush, context.Project(pos));
                    }
                    break;
                case "arc":
                    using (var pen = context.CreatePen(parameters.Skip(6)))
                    {
                        context.Canvas.DrawArc(pen,
                                               context.Project(pos),
                                               parameters.Get<float>(0),
                                               parameters.Get<float>(1));
                    }
                    break;
                case "pie":
                    using (var brush = context.CreateBrush(parameters.Skip(6)))
                    {
                        context.Canvas.FillPie(brush,
                                               context.Project(pos),
                                               parameters.Get<float>(0),
                                               parameters.Get<float>(1));
                    }
                    break;
                default:
                    throw new Exception($"Unknown method: { Method  }");
            }
        }
    }
}
