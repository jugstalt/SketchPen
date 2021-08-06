using SketchPen.Plot.Platform;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SketchPen.Plot.Extensions
{
    internal static class ParameterExtensions
    {
        static public T[] ToTypedParameters<T>(this IEnumerable<object> parameters)
        {
            return parameters?.Select(p =>
            {
                if (p?.GetType() == typeof(T))
                {
                    return p;
                }
                if (typeof(T) == typeof(float))
                {
                    return p.ToString().ToFloat();
                }
                if (typeof(T) == typeof(double))
                {
                    return p.ToString().ToDouble();
                }

                return Convert.ChangeType(p, typeof(T));
            }).Select(v => (T)v).ToArray();
        }

        static public T Get<T>(this IEnumerable<object> parameters, int index)
        {
            if (index < 0 || index > parameters.Count() - 1)
            {
                return default(T);
            }

            var p = parameters.Skip(index).First();
            if (p?.GetType() == typeof(T))
            {
                return (T)p;
            }
            if (typeof(T) == typeof(float))
            {
                return (T)(object)p.ToString().ToFloat();
            }
            if (typeof(T) == typeof(double))
            {
                return (T)(object)p.ToString().ToDouble();
            }

            return (T)Convert.ChangeType(p, typeof(T));
        }

        static public Color ToColor(this IEnumerable<object> parameters)
        {
            if (parameters == null || parameters.Count() == 0)
            {
                return Plotter.TransparentColor;
            }
            if (parameters.Count() == 1)
            {
                return ColorTranslator.FromHtml(parameters.Get<string>(0));
            }
            if (parameters.Count() == 2)
            {
                return Color.FromArgb(parameters.Get<int>(1), ColorTranslator.FromHtml(parameters.Get<string>(0)));
            }
            if (parameters.Count() == 3)
            {
                var rgb = parameters.ToTypedParameters<int>();
                return Color.FromArgb(rgb[0], rgb[1], rgb[2]);
            }
            if (parameters.Count() == 4)
            {
                var rgb = parameters.ToTypedParameters<int>();
                return Color.FromArgb(rgb[3], rgb[0], rgb[1], rgb[2]);
            }

            throw new Exception("Can't determine color from parameters");
        }

        static public RectangleF ToRectPos(this IEnumerable<object> parameters)
        {
            var coords = parameters.ToTypedParameters<float>();
            
            if (coords.Length == 1) {
                return new RectangleF(-coords[0] / 2f, -coords[0] / 2f, coords[0], coords[0]);
            }
            if (coords.Length == 2) {
                return new RectangleF(-coords[0] / 2f, -coords[0] / 2f, coords[0], coords[1]);
            }
            if (coords.Length == 3) {
                return new RectangleF(coords[1] - coords[0] / 2f, coords[2] - coords[0] / 2f, coords[0], coords[0]);
            }
            if (coords.Length >= 4)
            {
                return new RectangleF(coords[2] - coords[0] / 2f, coords[3] - coords[1] / 2f, coords[0], coords[1]);
            }

            throw new Exception("Can't determine position from parameters");
        }

        static public IEnumerable<PointF> ToPoints(this IEnumerable<object> parameters)
        {
            var coords = parameters.ToTypedParameters<float>();

            if (coords.Length % 2 != 0)
            {
                throw new Exception("Can't determine points from parameters. Number of parametes is not an even number.");
            }

            List<PointF> points = new List<PointF>();

            for (int i = 0; i < coords.Length; i += 2)
            {
                points.Add(new PointF(coords[i], coords[i + 1]));
            }

            return points;
        }

        static public Rectangle ToRectangle(this RectangleF rectangleF)
        {
            return new Rectangle((int)rectangleF.X, (int)rectangleF.Y, (int)rectangleF.Width, (int)rectangleF.Height);
        }
    }
}