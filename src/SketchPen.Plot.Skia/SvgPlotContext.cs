using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Skia.Extensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SketchPen.Plot.Skia;

/// <summary>
/// Vector (SVG) rendering backend. Draws the same plot commands as <see cref="PlotContext"/>,
/// but records them into an SKSvgCanvas instead of painting onto a bitmap.
/// </summary>
/// <remarks>
/// Unlike the raster <see cref="PlotContext"/>, whose <see cref="Encode"/> is a non-destructive,
/// repeatable read of an already-fully-drawn bitmap, SkiaSharp's SVG canvas only finalizes
/// (writes the closing &lt;/svg&gt; tag) when it is disposed. <see cref="Encode"/> therefore
/// disposes the drawing canvas as part of producing its output and can only be called once.
/// </remarks>
public class SvgPlotContext : IPlotContext
{
    private SKDynamicMemoryWStream _stream;
    private float _projectingFactor = 1f;
    private int _width, _height;
    private PlotContextOrigin _origin = PlotContextOrigin.Center;

    public SvgPlotContext()
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
        _stream = new SKDynamicMemoryWStream();
        var svgCanvas = SKSvgCanvas.Create(new SKRect(0, 0, width, height), _stream);

        _origin = origin;

        this.Canvas = new Canvas(svgCanvas);

        _projectingFactor = (float)(_width = width) / 100f;
        _height = height;

        this.ResetTransform();
        this.Init();
    }

    public void Dispose()
    {
        if (this.Canvas != null)
        {
            this.Canvas.Dispose();
            this.Canvas = null;
        }
        if (_stream != null)
        {
            _stream.Dispose();
            _stream = null;
        }
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

        SKPaint sKPaint;
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

    public IPlotPath CreatePlotPath()
    {
        return new PlotPath();
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

        switch (_origin)
        {
            case PlotContextOrigin.UpperLeft:
                ((Canvas)this.Canvas).SkCanvas.Translate(Project(0f), Project(0f));
                break;
            default:
                ((Canvas)this.Canvas).SkCanvas.Translate(Project(50f), Project(50f));
                break;
        }
    }

    /// <summary>
    /// Finalizes the SVG document and returns its bytes. Disposes the underlying drawing
    /// canvas as part of finalizing the SVG output (SkiaSharp only writes the closing
    /// &lt;/svg&gt; tag on disposal) — can only be called once per instance.
    /// </summary>
    public byte[] Encode(EncodeFormat format)
    {
        if (format != EncodeFormat.Svg)
        {
            throw new Exception($"Unsported image format: {format}");
        }

        if (this.Canvas == null)
        {
            throw new InvalidOperationException($"{nameof(SvgPlotContext)}.{nameof(Encode)}(...) can only be called once.");
        }

        // Disposing the SKSvgCanvas flushes the closing </svg> tag into the stream.
        this.Canvas.Dispose();
        this.Canvas = null;

        using (var data = _stream.DetachAsData())
        {
            return data.ToArray();
        }
    }

    #endregion
}
