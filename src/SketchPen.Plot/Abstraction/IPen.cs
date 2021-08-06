using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace SketchPen.Plot.Abstraction
{
    public interface IPen : IDisposable
    {
        Pen Pen { get; }
    }
}
