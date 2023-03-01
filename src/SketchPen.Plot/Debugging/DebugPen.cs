using SketchPen.Plot.Abstraction;

namespace SketchPen.Plot.Debugging;

internal class DebugPen : IPen
{
    private object _engineElement = new object();

    public object EngineElement => _engineElement;

    public void Dispose()
    {
    }
}
