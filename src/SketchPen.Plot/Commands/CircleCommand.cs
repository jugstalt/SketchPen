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
    [PlotCommandMethod("draw", suggestion: "draw(diameter)", snippet: "draw(${1:diameter});")]
    [PlotCommandMethod("draw", suggestion: "draw(diameterX, diameterY)", snippet: "draw(${1:diameterX}, ${2:diameterY});")]
    [PlotCommandMethod("draw", suggestion: "draw(diameter, centerX, centerY)", snippet: "draw(${1:diameter}, ${2:centerX}, ${3:centerY});")]
    [PlotCommandMethod("draw", suggestion: "draw(diameterX, diameterY, centerX, centerY)", snippet: "draw(${1:diameterX}, ${2:diameterY}, ${3:centerX}, ${4:centerY});")]
    [PlotCommandMethod("draw", suggestion: "draw(diameterX, diameterY, centerX, centerY, color)", snippet: "draw(${1:diameterX}, ${2:diameterY}, ${3:centerX}, ${4:centerY}, \"${5:hexColor}\");")]

    [PlotCommandMethod("fill", suggestion: "fill(diameter)", snippet: "fill(${1:diameter});")]
    [PlotCommandMethod("fill", suggestion: "fill(diameterX, diameterY)", snippet: "fill(${1:diameterX}, ${2:diameterY});")]
    [PlotCommandMethod("fill", suggestion: "fill(diameter, centerX, centerY)", snippet: "fill(${1:diameter}, ${2:centerX}, ${3:centerY});")]
    [PlotCommandMethod("fill", suggestion: "fill(diameterX, diameterY, centerX, centerY)", snippet: "fill(${1:diameterX}, ${2:diameterY}, ${3:centerX}, ${4:centerY});")]
    [PlotCommandMethod("fill", suggestion: "fill(diameterX, diameterY, centerX, centerY, color)", snippet: "fill(${1:diameterX}, ${2:diameterY}, ${3:centerX}, ${4:centerY}, \"${5:hexColor}\");")]

    [PlotCommandMethod("arc", suggestion: "arc(startAngle, sweepAngle, diameter)", snippet: "arc(${1:startAngle}, ${2:sweepAngle}, ${3:diameter});")]
    [PlotCommandMethod("arc", suggestion: "arc(startAngle, sweepAngle, diameterX, diameterY)", snippet: "arc(${1:startAngle}, ${2:sweepAngle}, ${3:diameterX}, ${4:diameterY});")]
    [PlotCommandMethod("arc", suggestion: "arc(startAngle, sweepAngle, diameter, centerX, centerY)", snippet: "arc(${1:startAngle}, ${2:sweepAngle}, ${3:diameter}, ${4:centerX}, ${5:centerY});")]
    [PlotCommandMethod("arc", suggestion: "arc(startAngle, sweepAngle, diameterX, diameterY, centerX, centerY)", snippet: "arc(${1:startAngle}, ${2:sweepAngle}, ${3:diameterX}, ${4:diameterY}, ${5:centerX}, ${6:centerY});")]
    [PlotCommandMethod("arc", suggestion: "arc(startAngle, sweepAngle, diameterX, diameterY, centerX, centerY, color)", snippet: "arc(${1:startAngle}, ${2:sweepAngle}, ${3:diameterX}, ${4:diameterY}, ${5:centerX}, ${6:centerY}, \"${7:hexColor}\");")]

    [PlotCommandMethod("pie", suggestion: "pie(startAngle, sweepAngle, diameter)", snippet: "pie(${1:startAngle}, ${2:sweepAngle}, ${3:diameter});")]
    [PlotCommandMethod("pie", suggestion: "pie(startAngle, sweepAngle, diameterX, diameterY)", snippet: "pie(${1:startAngle}, ${2:sweepAngle}, ${3:diameterX}, ${4:diameterY});")]
    [PlotCommandMethod("pie", suggestion: "pie(startAngle, sweepAngle, diameter, centerX, centerY)", snippet: "pie(${1:startAngle}, ${2:sweepAngle}, ${3:diameter}, ${4:centerX}, ${5:centerY});")]
    [PlotCommandMethod("pie", suggestion: "pie(startAngle, sweepAngle, diameterX, diameterY, centerX, centerY)", snippet: "pie(${1:startAngle}, ${2:sweepAngle}, ${3:diameterX}, ${4:diameterY}, ${5:centerX}, ${6:centerY});")]
    [PlotCommandMethod("pie", suggestion: "pie(startAngle, sweepAngle, diameterX, diameterY, centerX, centerY, color)", snippet: "pie(${1:startAngle}, ${2:sweepAngle}, ${3:diameterX}, ${4:diameterY}, ${5:centerX}, ${6:centerY}, \"${7:hexColor}\");")]

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
