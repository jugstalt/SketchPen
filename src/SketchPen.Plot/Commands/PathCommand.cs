using SketchPen.Parse.Lexer;
using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SketchPen.Plot.Commands
{
    [PlotCommandKeyword("path")]
    class PathCommand : GeneralPlotCommand
    {
        static private IPlotPath _path = null;

        protected override void ExecuteCommand(IPlotContext context, IEnumerable<object> parameters)
        {
            if (_path == null && Method?.ToLower() != "begin")
            {
                _path = context.CreatePlotPath();
                _path.Start();
            }

            switch (Method?.ToLower())
            {
                case "begin":
                    if (_path != null)
                    {
                        _path.Dispose();
                    }
                    _path = context.CreatePlotPath();
                    _path.Start();
                    break;
                case "start":
                    _path.Start();
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
                    _path.AddPoint(
                        parameters.ToPoints().Select(p => context.Project(p)).First());
                    break;
                case "close":
                    _path.Close();
                    break;
                case "draw":
                    using (var pen = context.CreatePen(parameters))
                    {
                        //CustomLineCap cap = new CustomLineCap(null, _path);
                        //cap.SetStrokeCaps(LineCap.Round, LineCap.Round);

                        //pen.CustomStartCap = cap;
                        //pen.CustomEndCap = cap;

                        context.Canvas.DrawPath(pen, _path);
                    }
                    break;
                case "fill":
                    using (var brush = context.CreateBrush(parameters))
                    {
                        context.Canvas.FillPath(brush, _path);
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
