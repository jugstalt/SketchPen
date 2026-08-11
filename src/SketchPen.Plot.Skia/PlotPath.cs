using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Skia.Extensions;
using SkiaSharp;
using System.Collections.Generic;

namespace SketchPen.Plot.Skia;

class PlotPath : IPlotPath
{
    private SKPath _path;
    private bool _startFigure = true;

    public PlotPath()
    {
        _path = new SKPath()
        {
            FillType = SKPathFillType.EvenOdd
        };

        _startFigure = true;
    }

    #region IPlotPath

    public object EngineElement => _path;

    public void Start()
    {
        _startFigure = true;
    }

    public void Close()
    {
        _path.Close();
        _startFigure = true;
    }

    public void AddArc(CanvasRectangle rect, float startAngle, float sweepAngle)
    {
        if (_startFigure)
        {
            _startFigure = false;
            _path.AddArc(rect.ToSKRect(), startAngle, sweepAngle);
        }
        else
        {
            _path.ArcTo(rect.ToSKRect(), startAngle, sweepAngle, false);
        }

        if (sweepAngle >= 360.0)
        {
            this.Close(); // to have the same behavoir as with System.Drawing
        }
    }

    public void AddLines(IEnumerable<CanvasPoint> points)
    {
        if (points == null)
        {
            return;
        }

        bool first = _startFigure;
        foreach (var point in points)
        {
            if (first)
            {
                _startFigure = first = false;
                _path.MoveTo(point.ToSKPoint());
            }
            else
            {
                _path.LineTo(point.ToSKPoint());
            }
        }
    }

    public void AddPoint(CanvasPoint point)
    {
        if (_path.PointCount == 0)
        {
            _startFigure = false;
            _path.MoveTo(point.ToSKPoint());
        }
        else
        {
            _path.LineTo(point.ToSKPoint());
        }
    }

    public void AddCubic(CanvasPoint controlPoint1, CanvasPoint controlPoint2, CanvasPoint end)
    {
        if (_startFigure)
        {
            throw new System.Exception("path.addcubic: no current point -- call addpoint/addlines/addarc first to establish a starting point.");
        }

        _path.CubicTo(controlPoint1.ToSKPoint(), controlPoint2.ToSKPoint(), end.ToSKPoint());
    }

    public void AddQuad(CanvasPoint controlPoint, CanvasPoint end)
    {
        if (_startFigure)
        {
            throw new System.Exception("path.addquad: no current point -- call addpoint/addlines/addarc first to establish a starting point.");
        }

        _path.QuadTo(controlPoint.ToSKPoint(), end.ToSKPoint());
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_path != null)
        {
            _path.Dispose();
            _path = null;
        }
    }

    #endregion
}
