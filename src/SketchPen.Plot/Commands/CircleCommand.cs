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
