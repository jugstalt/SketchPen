using System;

namespace SketchPen.Plot.Abstraction;

public interface IFont : IDisposable
{
    object EngineElement { get; }
}