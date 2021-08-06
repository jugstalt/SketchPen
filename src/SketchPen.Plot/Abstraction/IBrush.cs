using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SketchPen.Plot.Abstraction
{
    public interface IBrush : IDisposable
    {
        Brush Brush { get; }
    }
}
