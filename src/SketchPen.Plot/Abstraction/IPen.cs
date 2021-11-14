using System;
using System.Drawing;

namespace SketchPen.Plot.Abstraction
{
    public interface IPen : IDisposable
    {
        Pen Pen { get; }
    }
}
