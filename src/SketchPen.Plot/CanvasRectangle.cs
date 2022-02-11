using System;
using System.Collections.Generic;
using System.Text;

namespace SketchPen.Plot
{
    public struct CanvasRectangle
    {
        public CanvasRectangle(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
    }
}
