using System;
using System.Collections.Generic;
using System.Text;

namespace SketchPen.Plot
{
    public struct PlotColor
    {
        public byte A { get; private set; }
        public byte R { get; private set; }
        public byte G { get; private set; }
        public byte B { get; private set; }

        public int ToArgb() => (this.A << 24) | (this.R << 16) | (this.G << 8) | this.B;

        public bool IsTransparent => this.A == 0;

        public static PlotColor Empty => PlotColor.FromArgb(0, 0, 0, 0);
        public static PlotColor Transparent => PlotColor.FromArgb(0, 255, 255, 255);
        public static PlotColor White => PlotColor.FromArgb(255, 255, 255);
        public static PlotColor Black => PlotColor.FromArgb(0, 0, 0);
        public static PlotColor LightGray => PlotColor.FromArgb(200, 200, 200);
        public static PlotColor Gray => PlotColor.FromArgb(128, 128, 128);
        public static PlotColor Red => PlotColor.FromArgb(255, 0, 0);
        public static PlotColor Green => PlotColor.FromArgb(0, 255, 0);
        public static PlotColor Blue => PlotColor.FromArgb(0, 0, 255);
        public static PlotColor AliceBlue => PlotColor.FromArgb(240, 248, 255);
        public static PlotColor Yellow => PlotColor.FromArgb(255, 255, 0);
        public static PlotColor Cyan => PlotColor.FromArgb(0, 255, 255);
        public static PlotColor Orange => PlotColor.FromArgb(255, 165, 0);

        public static PlotColor FromArgb(int alpha, PlotColor baseColor)
        {
            return new PlotColor()
            {
                A = (byte)alpha,
                R = baseColor.R,
                G = baseColor.G,
                B = baseColor.B
            };
        }

        public static PlotColor FromArgb(int red, int green, int blue)
        {
            return new PlotColor()
            {
                A = 255,
                R = (byte)red,
                G = (byte)green,
                B = (byte)blue
            };
        }

        public static PlotColor FromArgb(int alpha, int red, int green, int blue)
        {
            return new PlotColor()
            {
                A = (byte)alpha,
                R = (byte)red,
                G = (byte)green,
                B = (byte)blue
            };
        }

        public static PlotColor FromArgb(int argb)
        {
            return PlotColor.FromArgb((byte)(argb >> 24),
                                      (byte)(argb >> 16),
                                      (byte)(argb >> 8),
                                      (byte)(argb));
        }

        public override bool Equals(object obj)
        {
            if (obj is PlotColor)
            {
                return ((PlotColor)obj).ToArgb() == this.ToArgb();
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return String.Join("; ", A < 255 ? new int[] { A, R, G, B } : new int[] { R, G, B });
        }
    }
}
