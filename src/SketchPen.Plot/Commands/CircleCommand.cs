using SketchPen.Parse.Lexer;
using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Reflection;
using System.Linq;
using System.Collections.Generic;
using SketchPen.Plot.Extensions;
using System.Drawing;

namespace SketchPen.Plot.Commands
{
    [PlotCommandKeyword("circle")]
    class CircleCommand : GeneralPlotCommand
    {
        protected override void ExecuteCommand(IPlotContext context, IEnumerable<object> parameters)
        {
            RectangleF pos;
            
            switch(Method?.ToLower())
            {
                case "arc":
                case "pie":
                    pos = parameters.Skip(2).ToRectPos();
                    break;
                default:
                    pos = parameters.ToRectPos();
                    break;
            }
            

            switch(Method?.ToLower())
            {
                case "draw":
                    using(var pen = context.CreatePen(parameters.Skip(4)))
                    {
                        context.GraphicsContext.DrawEllipse(pen.Pen, context.Project(pos));
                    }
                    break;
                case "fill":
                    using (var brush = context.CreateBrush(parameters.Skip(4)))
                    {
                        context.GraphicsContext.FillEllipse(brush.Brush, context.Project(pos));
                    }
                    break;
                case "arc":    
                    using (var pen = context.CreatePen(parameters.Skip(6)))
                    {
                        context.GraphicsContext.DrawArc(pen.Pen, 
                                                        context.Project(pos),
                                                        parameters.Get<float>(0),
                                                        parameters.Get<float>(1));
                    }
                    break;
                case "pie":
                    using (var brush = context.CreateBrush(parameters.Skip(6)))
                    {
                        context.GraphicsContext.FillPie(brush.Brush,
                                                        context.Project(pos).ToRectangle(),
                                                        parameters.Get<float>(0),
                                                        parameters.Get<float>(1));
                    }
                    break;
            }
        }
    }
}
