using SketchPen.Plot.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace SketchPen.Plot.Debug;

internal class DebugBrush : IBrush
{
    private object _engineElement = new object();

    public object EngineElement => _engineElement;

    public void Dispose()
    {
    }
}
