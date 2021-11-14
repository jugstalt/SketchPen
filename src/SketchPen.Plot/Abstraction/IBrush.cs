using System;
using System.Drawing;

namespace SketchPen.Plot.Abstraction
{
    public interface IBrush : IDisposable
    {
        Brush Brush { get; }
    }
}
