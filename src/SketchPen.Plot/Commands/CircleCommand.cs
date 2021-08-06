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
        public override void Execute(IPlotContext context)
        {
            RectangleF pos;
            
            switch(Method?.ToLower())
            {
                case "arc":
                case "pie":
                    pos = Parameters.Skip(2).ToRectPos();
                    break;
                default:
                    pos = Parameters.ToRectPos();
                    break;
            }
            

            switch(Method?.ToLower())
            {
                case "draw":
                    using(var pen = context.CreatePen())
                    {
                        context.GraphicsContext.DrawEllipse(pen.Pen, context.Project(pos));
                    }
                    break;
                case "fill":
                    using (var brush = context.CreateBrush())
                    {
                        context.GraphicsContext.FillEllipse(brush.Brush, context.Project(pos));
                    }
                    break;
                case "arc":    
                    using (var pen = context.CreatePen())
                    {
                        context.GraphicsContext.DrawArc(pen.Pen, 
                                                        context.Project(pos),
                                                        Parameters.Get<float>(0),
                                                        Parameters.Get<float>(1));
                    }
                    break;
                case "pie":
                    using (var brush = context.CreateBrush())
                    {
                        context.GraphicsContext.FillPie(brush.Brush,
                                                        context.Project(pos).ToRectangle(),
                                                        Parameters.Get<float>(0),
                                                        Parameters.Get<float>(1));
                    }
                    break;
            }
        }
    }
}
