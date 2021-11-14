using SketchPen.Parse.Lexer;
using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;

namespace SketchPen.Plot.Commands
{
    [PlotCommandKeyword("path")]
    class PathCommand : GeneralPlotCommand
    {
        static private GraphicsPath _path = null;

        protected override void ExecuteCommand(IPlotContext context, IEnumerable<object> parameters)
        {
            if (_path == null && Method?.ToLower() != "begin")
            {
                _path = new GraphicsPath();
                _path.StartFigure();
            }

            switch (Method?.ToLower())
            {
                case "begin":
                    if (_path != null)
                    {
                        _path.Dispose();
                    }
                    _path = new GraphicsPath();
                    _path.StartFigure();
                    break;
                case "start":
                    _path.StartFigure();
                    break;
                case "addlines":
                    _path.AddLines(parameters.ToPoints().Select(p => context.Project(p)).ToArray());
                    break;
                case "addarc":
                    var pos = parameters.Skip(2).ToRectPos();
                    _path.AddArc(context.Project(pos),
                                 parameters.Get<float>(0),
                                 parameters.Get<float>(1));
                    break;
                case "addpoint":
                    _path.AddLine(
                        parameters.ToPoints().Select(p => context.Project(p)).First(),
                        parameters.ToPoints().Select(p => context.Project(p)).First());
                    break;
                case "close":
                    _path.CloseFigure();
                    break;
                case "draw":
                    using (var pen = context.CreatePen(parameters))
                    {
                        //CustomLineCap cap = new CustomLineCap(null, _path);
                        //cap.SetStrokeCaps(LineCap.Round, LineCap.Round);

                        //pen.CustomStartCap = cap;
                        //pen.CustomEndCap = cap;

                        context.GraphicsContext.DrawPath(pen.Pen, _path);
                    }
                    break;
                case "fill":
                    using (var brush = context.CreateBrush(parameters))
                    {
                        context.GraphicsContext.FillPath(brush.Brush, _path);
                    }
                    break;
                default:
                    throw new Exception($"Unknown method: { Method  }");
            }
        }

        public override void Init(IEnumerable<Token> statement)
        {
            base.Init(statement);

            if (_path != null)
            {
                _path.Dispose();
                _path = null;
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
