using SketchPen.Plot.Abstraction;

namespace SketchPen.Plot.Debugging;

internal class DebugBrush : IBrush
{
    private object _engineElement = new object();

    public object EngineElement => _engineElement;

    public void Dispose()
    {
    }
}
