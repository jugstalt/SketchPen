using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Drawing.Extensions;
using System;
using System.Drawing;

namespace SketchPen.Plot.Drawing;

internal class PlotBrush : IBrush
{
    private readonly PlotContext _plotContext;
    private readonly Canvas _graphicsContext;
    private readonly bool _isPseudeoTransparent;

    public PlotBrush(PlotContext context, Brush brush, bool isPseudoTransparent)
    {
        _plotContext = context;
        _graphicsContext = (Canvas)context.Canvas;

        _isPseudeoTransparent = isPseudoTransparent;

        if (_isPseudeoTransparent)
        {
            _graphicsContext.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        }

        this.EngineElement = brush;
    }

    public object EngineElement { get; }

    public void Dispose()
    {
        if (this.EngineElement is IDisposable)
        {
            ((IDisposable)this.EngineElement).Dispose();
        }

        if (_isPseudeoTransparent)
        {
            _plotContext.Bitmap.MakeTransparent(PlotColor.Transparent.ToColor());
            _graphicsContext.Graphics.SmoothingMode = PlotContext.DefaultSmothingMode;
        }
    }
}