using SketchPen.Plot.Platform;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SketchPen.Plot.Extensions
{
    public static class ParameterExtensions
    {
        static public T[] ToTypedParameters<T>(this IEnumerable<object> parameters)
        {
            return parameters.Select(p =>
            {
                if (p.GetType() == typeof(T))
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

        static public T[] TakeFirstTypedParameterBlock<T>(this IEnumerable<object> parameters)
        {
            List<T> typedParamters = new List<T>();

            foreach (var parameter in parameters)
            {
                try
                {
                    if (parameter.GetType() == typeof(T))
                    {
                        typedParamters.Add((T)parameter);
                    }
                    else if (typeof(T) == typeof(float))
                    {
                        typedParamters.Add((T)(object)parameter.ToString().ToFloat());
                    }
                    else if (typeof(T) == typeof(double))
                    {
                        typedParamters.Add((T)(object)parameter.ToString().ToDouble());
                    }
                    else
                    {
                        typedParamters.Add((T)Convert.ChangeType(parameter, typeof(T)));
                    }
                }
                catch
                {
                    break;
                }
            }

            return typedParamters.ToArray();
        }

        static public T Get<T>(this IEnumerable<object> parameters, int index)
        {
            if (index < 0 || index > parameters.Count() - 1)
            {
                return GetDefaultNonNullValue<T>();
            }

            var p = parameters.Skip(index).First();

            if (p.GetType() == typeof(T))
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

        static public PlotColor ToColor(this IEnumerable<object> parameters)
        {
            if (parameters == null || parameters.Count() == 0)
            {
                return PlotColor.Transparent;
            }
            if (parameters.Count() == 1)
            {
               return PlotColor.FromHtml(parameters.Get<string>(0));
            }
            if (parameters.Count() == 2)
            {
                var col = PlotColor.FromHtml(parameters.Get<string>(0));
                return PlotColor.FromArgb(parameters.Get<int>(1), col.R, col.G, col.B);
            }
            if (parameters.Count() == 3)
            {
                var rgb = parameters.ToTypedParameters<int>();
                return PlotColor.FromArgb(rgb[0], rgb[1], rgb[2]);
            }
            if (parameters.Count() == 4)
            {
                var rgb = parameters.ToTypedParameters<int>();
                return PlotColor.FromArgb(rgb[3], rgb[0], rgb[1], rgb[2]);
            }

            throw new Exception("Can't determine color from parameters");
        }

        static public CanvasRectangle ToRectPos(this IEnumerable<object> parameters)
        {
            var coords = parameters.TakeFirstTypedParameterBlock<float>();

            if (coords.Length == 1)
            {
                return new CanvasRectangle(-coords[0] / 2f, -coords[0] / 2f, coords[0], coords[0]);
            }
            if (coords.Length == 2)
            {
                return new CanvasRectangle(-coords[0] / 2f, -coords[1] / 2f, coords[0], coords[1]);
            }
            if (coords.Length == 3)
            {
                return new CanvasRectangle(coords[1] - coords[0] / 2f, coords[2] - coords[0] / 2f, coords[0], coords[0]);
            }
            if (coords.Length >= 4)
            {
                return new CanvasRectangle(coords[2] - coords[0] / 2f, coords[3] - coords[1] / 2f, coords[0], coords[1]);
            }

            throw new Exception("Can't determine position from parameters");
        }

        static public IEnumerable<CanvasPoint> ToPoints(this IEnumerable<object> parameters)
        {
            var coords = parameters.TakeFirstTypedParameterBlock<float>();

            if (coords.Length % 2 != 0)
            {
                throw new Exception("Can't determine points from parameters. Number of parametes is not an even number.");
            }

            List<CanvasPoint> points = new List<CanvasPoint>();

            for (int i = 0; i < coords.Length; i += 2)
            {
                points.Add(new CanvasPoint(coords[i], coords[i + 1]));
            }

            return points;
        }

        

        static public IEnumerable<object> Slice(this IEnumerable<object> parameters, int from, int count = 1)
        {
            if (parameters == null || from >= parameters.Count())
            {
                return Array.Empty<object>();
            }

            return parameters.Skip(from).Take(1);
        }

        static public int CountElements(this IEnumerable<object> parameters)
        {
            if (parameters == null)
            {
                return 0;
            }

            return parameters.Count();
        }

        static public T GetDefaultNonNullValue<T>()
        {
            return typeof(T) switch
            {
                Type t when t == typeof(string) => (T)(object)string.Empty,
                Type t when t == typeof(short) => (T)(object)default(short),
                Type t when t == typeof(int) => (T)(object)default(int),
                Type t when t == typeof(long) => (T)(object)default(long),
                Type t when t == typeof(float) => (T)(object)default(float),
                Type t when t == typeof(double) => (T)(object)default(double),
                Type t when t == typeof(decimal) => (T)(object)default(decimal),
                _ => throw new ArgumentException($"Type '{typeof(T).FullName}' is not supported"),
            };
        }
    }
}