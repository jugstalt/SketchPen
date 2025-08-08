using SketchPen.Plot.Abstraction;

namespace SketchPen.Plot.Debugging;

internal class DebugFont : IFont
{
    private object _engineElement = new object();
    public object EngineElement => _engineElement;
    public void Dispose()
    {
    }
}
