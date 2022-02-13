using System;
using System.Drawing;

namespace SketchPen.Plot.Abstraction
{
    public interface IPen : IDisposable
    {
        object EngineElement { get; }
    }
}
