using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Drawing.Extensions;
using SketchPen.Plot.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;

namespace SketchPen.Plot.Drawing;

public class PlotContext : IPlotContext
{
    static internal System.Drawing.Drawing2D.SmoothingMode DefaultSmothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

    private bool _disposeBitmap = false;
    private Bitmap _bitmap;
    private float _projectingFactor = 1f;
    private int _width, _height;
    private PlotContextOrigin _origin;

    public PlotContext()
    {
        this.Globals = new Dictionary<string, object>();
    }

    private void Init()
    {
        this.GradientBrushColor = PlotColor.Transparent;
        this.GradientBrushPoint2 = new CanvasPoint(_width / 2f, _height / 2f);
        this.GradientBrushPoint1 = new CanvasPoint(-_width / 2f, -_height / 2f);

        MinPenWidth = 1f;
        MaxPenWidth = float.MaxValue;
        PenCap = PenCap.Round;
    }

    internal Bitmap Bitmap => _bitmap;

    #region IPlotContext

    public void Init(int width, int height, PlotContextOrigin origin = PlotContextOrigin.Center)
    {
        _disposeBitmap = true;
        _bitmap = new Bitmap(_width = width, _height = height);
        _bitmap.SetResolution(96f, 96f);
        _bitmap.MakeTransparent();
        _origin = origin;
        _projectingFactor = (float)width / 100f;

        this.Canvas = new Canvas(_bitmap);
        this.ResetTransform();
        this.Init();
    }

    public ICanvas Canvas { get; private set; }

    public PlotColor PenColor { private get; set; }
    public float PenWidth { private get; set; }
    public PenCap PenCap { private get; set; }
    public float MaxPenWidth { private get; set; }
    public float MinPenWidth { private get; set; }


    public PlotColor BrushColor { private get; set; }

    public PlotColor GradientBrushColor { private get; set; }
    public CanvasPoint GradientBrushPoint1 { private get; set; }
    public CanvasPoint GradientBrushPoint2 { private get; set; }

    public IDictionary<string, object> Globals { get; }

    public IPen CreatePen(IEnumerable<object> parameters = null)
    {
        var typedParameters = parameters?.Slice(1)?.ToTypedParameters<float>();
        float penWidth = typedParameters != null && typedParameters.Length >= 1 ?
            typedParameters.First() :
            this.PenWidth;

        if (penWidth >= MinPenWidth && penWidth <= MaxPenWidth)
        {
            penWidth = Project(this.PenWidth);
            if (penWidth < MinPenWidth)
            {
                penWidth = MinPenWidth;
            }
            else if (penWidth > MaxPenWidth)
            {
                penWidth = MaxPenWidth;
            }
        }

        var penColor = parameters.Slice(0)?.ToColor() ?? this.PenColor;
        var pen = new Pen(penColor.ToColor(), penWidth);

        switch (this.PenCap)
        {
            case PenCap.Round:
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.LineJoin = LineJoin.Round;
                break;
            case PenCap.Square:
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Square;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Square;
                pen.LineJoin = LineJoin.Bevel;
                break;
            case PenCap.Flat:
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Flat;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Flat;
                pen.LineJoin = LineJoin.Bevel;
                break;
        }

        return new PlotPen(this, pen, penColor.Equals(PlotColor.Transparent));
    }

    public IBrush CreateBrush(IEnumerable<object> parameters = null)
    {
        var brushColor = this.BrushColor;
        var gradientBrushColor = this.GradientBrushColor;

        if (parameters.CountElements() == 1)
        {
            brushColor = parameters.Slice(0).ToColor();
            gradientBrushColor = PlotColor.Transparent;
        }
        else if (parameters.CountElements() == 2)
        {
            brushColor = parameters.Slice(0).ToColor();
            gradientBrushColor = parameters.Slice(1).ToColor();
        }

        Brush brush = null;
        if (!gradientBrushColor.Equals(PlotColor.Transparent) &&
           this.GradientBrushPoint1 != null &&
           this.GradientBrushPoint2 != null)
        {
            brush = new LinearGradientBrush(
                this.GradientBrushPoint1.ToPoint(),
                this.GradientBrushPoint2.ToPoint(),
                brushColor.ToColor(),
                gradientBrushColor.ToColor());
        }
        else
        {
            brush = new SolidBrush(brushColor.ToColor());
        }

        return new PlotBrush(this, brush, brushColor.Equals(PlotColor.Transparent));
    }

    public IFont CreateFont(IEnumerable<object> parameters)
    {
        var size = 12f;
        var fontFamily = "Arial";

        if (parameters?.CountElements() == 1)
        {
            if (parameters.First() is string fontName)
            {
                fontFamily = fontName;
            }
            else if (parameters.First() is float fontSize)
            {
                size = fontSize;
            }
        }
        else if (parameters?.CountElements() >= 2)
        {
            fontFamily = parameters.ElementAtOrDefault(0)?.ToString() ?? "Arial";
            size = (float)parameters.ElementAtOrDefault(1);
        }

        return new PlotFont(this, new Font(fontFamily, Project(size)));
    }

    public CanvasPoint Project(CanvasPoint point)
    {
        return new CanvasPoint(point.X * _projectingFactor,
                               point.Y * _projectingFactor);
    }

    public CanvasRectangle Project(CanvasRectangle rect)
    {
        return new CanvasRectangle(
            rect.X * _projectingFactor,
            rect.Y * _projectingFactor,
            rect.Width * _projectingFactor,
            rect.Height * _projectingFactor);
    }

    public float Project(float number)
    {
        return number * _projectingFactor;
    }

    public void ResetTransform()
    {
        ((Canvas)this.Canvas).Graphics.ResetTransform();

        switch (_origin)
        {
            case PlotContextOrigin.UpperLeft:
                ((Canvas)this.Canvas).Graphics.TranslateTransform(Project(0f), Project(0f));
                break;
            default:
                ((Canvas)this.Canvas).Graphics.TranslateTransform(Project(50f), Project(50f));
                break;
        }

    }
    public IPlotPath CreatePlotPath() => new PlotPath();

    public byte[] Encode(EncodeFormat format)
    {
        if (_bitmap != null)
        {
            var imgFormat = System.Drawing.Imaging.ImageFormat.Png;

            switch (format)
            {
                case EncodeFormat.Jpeg:
                    imgFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                    break;
            }

            var ms = new MemoryStream();
            _bitmap.Save(ms, imgFormat);

            return ms.ToArray();
        }

        return null;
    }

    #endregion IPlotContext

    #region IDispable

    public void Dispose()
    {
        if (this.Canvas != null)
        {
            this.Canvas.Dispose();
            this.Canvas = null;
        }

        if (_disposeBitmap == true && _bitmap != null)
        {
            _bitmap.Dispose();
            _bitmap = null;
        }
    }

    #endregion
}
