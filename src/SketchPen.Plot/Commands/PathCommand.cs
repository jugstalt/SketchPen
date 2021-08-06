using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Reflection;
using System;
using System.Drawing.Drawing2D;
using System.Linq;

namespace SketchPen.Plot.Commands
{
    [PlotCommandKeyword("path")]
    class PathCommand : GeneralPlotCommand
    {
        static private GraphicsPath _path = null;

        public override void Execute(IPlotContext context)
        {
            switch (Method?.ToLower())
            {
                case "start":
                    if (_path != null)
                    {
                        _path.Dispose();
                    }

                    _path = new GraphicsPath();
                    _path.StartFigure();
                    break;
                case "addlines":
                    if (_path == null)
                    {
                        throw new Exception("Start path before add points");
                    }

                    _path.AddLines(Parameters.ToPoints().Select(p => context.Project(p)).ToArray());
                    break;
                case "draw":
                    if (_path == null)
                    {
                        throw new Exception("Start path before draw it");
                    }

                    using (var pen = context.CreatePen())
                    {
                        //CustomLineCap cap = new CustomLineCap(null, _path);
                        //cap.SetStrokeCaps(LineCap.Round, LineCap.Round);

                        //pen.CustomStartCap = cap;
                        //pen.CustomEndCap = cap;

                        context.GraphicsContext.DrawPath(pen.Pen, _path);
                    }
                    break;
                case "fill":
                    if (_path == null)
                    {
                        throw new Exception("Start path before fill it");
                    }

                    using (var brush = context.CreateBrush())
                    {
                        context.GraphicsContext.FillPath(brush.Brush, _path);
                    }
                    break;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_path != null)
            {
                _path.Dispose();
                _path = null;
            }
        }
    }
}
