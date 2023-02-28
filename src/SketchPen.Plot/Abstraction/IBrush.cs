using System;

namespace SketchPen.Plot.Abstraction;

public interface IBrush : IDisposable
{
    object EngineElement { get; }
}
