using System;

namespace SketchPen.Plot.Abstraction;

public interface IPen : IDisposable
{
    object EngineElement { get; }
}
