using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Skia.Extensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SketchPen.Plot.Skia
{
    public class PlotContext : IPlotContext
    {
        private bool _disposeBitmap = false;
        private SKBitmap _bitmap;
        private float _projectingFactor = 1f;
        private int _width, _height;

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

        public void Init(int width, int height)
        {
            _disposeBitmap = true;

            //_bitmap = new SKBitmap(_width = width, _height = height, true);
            _bitmap = new SKBitmap(_width = width, _height = height, 
                                   colorType: SKColorType.Bgra8888, 
                                   alphaType: SKAlphaType.Premul);

            this.Canvas = new Canvas(_bitmap);
            _projectingFactor = _bitmap.Width / 100f;

            this.ResetTransform();

            Init();
        }

        public void Init(int width, int height, object canvasObject)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if(this.Canvas != null)
            {
                this.Canvas.Dispose();
                this.Canvas = null;
            }
            if(_disposeBitmap && _bitmap != null)
            {
                _bitmap.Dispose();
                _bitmap = null;
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

            SKPaint sKPaint = null;
            if (!gradientBrushColor.Equals(PlotColor.Transparent) &&
               this.GradientBrushPoint1 != null &&
               this.GradientBrushPoint2 != null)
            {
                sKPaint = new SKPaint()
                {
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
                    Style = SKPaintStyle.Fill
                };
            }

            return new PlotBrush(this, sKPaint, brushColor.Equals(PlotColor.Transparent));
        }

        public IPen CreatePen(IEnumerable<object> parameters = null)
        {
            float penWidth = parameters.Slice(1)?.ToTypedParameters<float>().FirstOrDefault() ?? this.PenWidth;

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
                    throw new Exception($"Unsported image format: { format }");
            }

            return skData?.ToArray();
        }

        public CanvasPoint Project(CanvasPoint point)
        {
            return new CanvasPoint(point.X * _projectingFactor, point.Y * _projectingFactor);
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
            ((Canvas)this.Canvas).SkCanvas.Translate(Project(50f), Project(50f));
        }

        #endregion
    }
}
