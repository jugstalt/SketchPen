using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SketchPen.Plot.Commands;

[PlotCommandKeyword("path")]
[PlotCommandSupportedFileTypes(EditorFileType.Template | EditorFileType.Code)]
[PlotCommandMethod("begin", snippet: "begin();")]
[PlotCommandMethod("start", snippet: "start();")]
[PlotCommandMethod("close", snippet: "close();")]

[PlotCommandMethod("addlines", snippet: "addlines(${1:x1},${2:y1}, ${3:x2},${4:y2}, ${5:...});")]

[PlotCommandMethod("addarc", suggestion: "addarc(startAngle, sweepAngle, diameter)", snippet: "addarc(${1:startAngle}, ${2:sweepAngle}, ${3:diameter});")]
[PlotCommandMethod("addarc", suggestion: "addarc(startAngle, sweepAngle, diameterX, diameterY)", snippet: "addarc(${1:startAngle}, ${2:sweepAngle}, ${3:diameterX}, ${4:diameterY});")]
[PlotCommandMethod("addarc", suggestion: "addarc(startAngle, sweepAngle, diameter, centerX, centerY)", snippet: "addarc(${1:startAngle}, ${2:sweepAngle}, ${3:diameter}, ${4:centerX}, ${5:centerY});")]
[PlotCommandMethod("addarc", suggestion: "addarc(startAngle, sweepAngle, diameterX, diameterY, centerX, centerY)", snippet: "addarc(${1:startAngle}, ${2:sweepAngle}, ${3:diameterX}, ${4:diameterY}, ${5:centerX}, ${6:centerY});")]

[PlotCommandMethod("addpoint", snippet: "addpoint(${1:x}, ${2:y});")]

[PlotCommandMethod("addcubic", suggestion: "addcubic(cp1x, cp1y, cp2x, cp2y, x, y)", snippet: "addcubic(${1:cp1x}, ${2:cp1y}, ${3:cp2x}, ${4:cp2y}, ${5:x}, ${6:y});")]
[PlotCommandMethod("addquad", suggestion: "addquad(cpx, cpy, x, y)", snippet: "addquad(${1:cpx}, ${2:cpy}, ${3:x}, ${4:y});")]

[PlotCommandMethod("draw", suggestion: "draw()", snippet: "draw();")]
[PlotCommandMethod("draw", suggestion: "draw(color)", snippet: "draw(\"${1:hexColor}\");")]

[PlotCommandMethod("fill", suggestion: "fill()", snippet: "fill();")]
[PlotCommandMethod("fill", suggestion: "fill(color)", snippet: "fill(\"${1:hexColor}\");")]


class PathCommand : GeneralPlotCommand
{
    // The "current" path lives on IPlotContext (context.CurrentPath), not as a field here.
    // A single compiled command list is reused across multiple Plot() calls (e.g. once per
    // output size, see Plotter.Plot()), but a fresh IPlotContext is created for each one --
    // storing it on the context is what makes "path.begin() is optional at the very start of a
    // render" actually safe, instead of leaking an already-projected (pixel-scale-specific)
    // path from one render's canvas size into the next.
    protected override void ExecuteCommand(IPlotContext context, IEnumerable<object> parameters)
    {
        if (context.CurrentPath == null && Method?.ToLower() != "begin")
        {
            context.CurrentPath = context.CreatePlotPath();
            context.CurrentPath.Start();
        }

        switch (Method?.ToLower())
        {
            case "begin":
                context.CurrentPath?.Dispose();
                context.CurrentPath = context.CreatePlotPath();
                context.CurrentPath.Start();
                break;
            case "start":
                context.CurrentPath?.Start();
                break;
            case "addlines":
                context.CurrentPath?.AddLines(parameters.ToPoints().Select(p => context.Project(p)).ToArray());
                break;
            case "addarc":
                var pos = parameters.Skip(2).ToRectPos();
                context.CurrentPath?.AddArc(context.Project(pos),
                             parameters.Get<float>(0),
                             parameters.Get<float>(1));
                break;
            case "addpoint":
                context.CurrentPath?.AddPoint(
                    parameters.ToPoints().Select(p => context.Project(p)).First());
                break;
            case "addcubic":
                var cubic = parameters.ToPoints().Select(p => context.Project(p)).ToArray();
                context.CurrentPath?.AddCubic(cubic[0], cubic[1], cubic[2]);
                break;
            case "addquad":
                var quad = parameters.ToPoints().Select(p => context.Project(p)).ToArray();
                context.CurrentPath?.AddQuad(quad[0], quad[1]);
                break;
            case "close":
                context.CurrentPath?.Close();
                break;
            case "draw":
                if (context.CurrentPath != null)
                {
                    using (var pen = context.CreatePen(parameters))
                    {
                        //CustomLineCap cap = new CustomLineCap(null, _path);
                        //cap.SetStrokeCaps(LineCap.Round, LineCap.Round);

                        //pen.CustomStartCap = cap;
                        //pen.CustomEndCap = cap;

                        context.Canvas.DrawPath(pen, context.CurrentPath);
                    }
                }

                break;
            case "fill":
                if (context.CurrentPath != null)
                {
                    using (var brush = context.CreateBrush(parameters))
                    {
                        context.Canvas.FillPath(brush, context.CurrentPath);
                    }
                }

                break;
            default:
                throw new Exception($"Unknown method: {Method}");
        }
    }
}
