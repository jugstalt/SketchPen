using System;
using System.Collections.Generic;

namespace SketchPen.Plot.Abstraction;

public interface IPlotContext : IDisposable
{
    void Init(int width, int height, PlotContextOrigin origin = PlotContextOrigin.Center);

    ICanvas Canvas { get; }

    PlotColor PenColor { set; }
    float PenWidth { set; }
    PenCap PenCap { set; }

    float MaxPenWidth { set; }
    float MinPenWidth { set; }

    PlotColor BrushColor { set; }

    PlotColor GradientBrushColor { set; }
    CanvasPoint GradientBrushPoint1 { set; }
    CanvasPoint GradientBrushPoint2 { set; }

    IDictionary<string, object> Globals { get; }

    IPen CreatePen(IEnumerable<object> parameters);
    IBrush CreateBrush(IEnumerable<object> parameters);

    IFont CreateFont(IEnumerable<object> parameters);

    CanvasPoint Project(CanvasPoint point);
    CanvasRectangle Project(CanvasRectangle rect);

    IPlotPath CreatePlotPath();

    /// <summary>
    /// The path currently being built by <c>path.*</c> commands (see PathCommand), if any.
    /// Lives here -- scoped to one <see cref="IPlotContext"/>, i.e. one render/Plot() call --
    /// rather than as a field on PathCommand itself, since a single compiled command list is
    /// reused across multiple Plot() calls (e.g. once per output size); a static field there
    /// would leak an already-projected (pixel-scale-specific) path from one render's canvas
    /// size into the next.
    /// </summary>
    IPlotPath? CurrentPath { get; set; }

    float Project(float number);

    void ResetTransform();

    byte[] Encode(EncodeFormat format);
}
