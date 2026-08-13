using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Skia.Extensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SketchPen.Plot.Skia;

public class PlotContext : IPlotContext
{
    private bool _disposeBitmap = false;
    private SKBitmap _bitmap;
    private float _projectingFactor = 1f;
    private int _width, _height;
    private PlotContextOrigin _origin = PlotContextOrigin.Center;

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

    #region IPlotContext

    public void Init(int width, int height, PlotContextOrigin origin = PlotContextOrigin.Center)
    {
        _disposeBitmap = true;

        //_bitmap = new SKBitmap(_width = width, _height = height, true);
        _bitmap = new SKBitmap(_width = width, _height = height,
                               colorType: SKColorType.Bgra8888,
                               alphaType: SKAlphaType.Premul);
        _origin = origin;

        this.Canvas = new Canvas(new SKCanvas(_bitmap));

        _projectingFactor = (float)width / 100f;

        this.ResetTransform();
        this.Init();
    }

    public void Dispose()
    {
        if (this.CurrentPath != null)
        {
            this.CurrentPath.Dispose();
            this.CurrentPath = null;
        }
        if (this.Canvas != null)
        {
            this.Canvas.Dispose();
            this.Canvas = null;
        }
        if (_disposeBitmap && _bitmap != null)
        {
            _bitmap.Dispose();
            _bitmap = null;
        }
    }

    public ICanvas Canvas { get; private set; }

    public IPlotPath CurrentPath { get; set; }

    internal SKBitmap Bitmap => _bitmap;

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

        SKPaint sKPaint = null;
        if (!gradientBrushColor.Equals(PlotColor.Transparent) &&
           this.GradientBrushPoint1 != null &&
           this.GradientBrushPoint2 != null)
        {
            sKPaint = new SKPaint()
            {
                IsAntialias = true,
                Shader = SKShader.CreateLinearGradient(
                    GradientBrushPoint1.ToSKPoint(),
                    GradientBrushPoint2.ToSKPoint(),
                    new SKColor[] { brushColor.ToSKColor(), gradientBrushColor.ToSKColor() },
                    new float[] { 0, 1 },
                    SKShaderTileMode.Clamp)
            };
        }
        else
        {
            sKPaint = new SKPaint()
            {
                ColorF = brushColor.ToSKColor(),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
        }

        return new PlotBrush(this, sKPaint, brushColor.Equals(PlotColor.Transparent));
    }

    public IPen CreatePen(IEnumerable<object> parameters)
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

        var colorParams = parameters.Slice(0);
        var penColor = (colorParams == null || colorParams.Count() == 0) ?
            this.PenColor :
            colorParams.ToColor();

        var skPaint = new SKPaint()
        {
            Color = penColor.ToSKColor(),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = Math.Max(0.8f, penWidth),
            StrokeJoin = SKStrokeJoin.Round,
            IsAntialias = true
        };

        switch (this.PenCap)
        {
            case PenCap.Round:
                skPaint.StrokeJoin = SKStrokeJoin.Round;
                skPaint.StrokeCap = SKStrokeCap.Round;
                break;
            case PenCap.Square:
                skPaint.StrokeJoin = SKStrokeJoin.Miter;
                skPaint.StrokeCap = SKStrokeCap.Square;
                break;
            case PenCap.Flat:
                skPaint.StrokeJoin = SKStrokeJoin.Bevel;
                skPaint.StrokeCap = SKStrokeCap.Butt;
                break;
        }

        return new PlotPen(this, skPaint);
    }

    public IPlotPath CreatePlotPath()
    {
        return new PlotPath();
    }

    public byte[] Encode(EncodeFormat format)
    {
        var image = SKImage.FromBitmap(_bitmap);
        int quality = 0;

        SKData skData = null;
        switch (format)
        {
            case EncodeFormat.Png:
                skData = image.Encode(SKEncodedImageFormat.Png, quality > 0 ? quality : 75);
                break;
            case EncodeFormat.Jpeg:
                skData = image.Encode(SKEncodedImageFormat.Jpeg, quality > 0 ? quality : 75);
                break;
            default:
                throw new Exception($"Unsported image format: {format}");
        }

        return skData?.ToArray();
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
        ((Canvas)this.Canvas).SkCanvas.ResetMatrix();

        switch(_origin)
        {
            case PlotContextOrigin.UpperLeft:
                ((Canvas)this.Canvas).SkCanvas.Translate(Project(0f), Project(0f));
                break;
            default:
                ((Canvas)this.Canvas).SkCanvas.Translate(Project(50f), Project(50f));
                break;
        }
    }

    public IFont CreateFont(IEnumerable<object> parameters)
    {
        // implementation for creating a font
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

        var skTypeface = SKTypeface.FromFamilyName(fontFamily);
        if (skTypeface == null)
        {
            skTypeface = SKTypeface.Default;
        }
        var skFont = new SKFont(skTypeface, Project(size))
        {
            Edging = SKFontEdging.Antialias
        };

        return new PlotFont(this, skFont);
    }

    #endregion
}
