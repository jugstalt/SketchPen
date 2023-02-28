namespace SketchPen.Plot;

public struct CanvasPoint
{
    public CanvasPoint(float x, float y)
    {
        X = x;
        Y = y;
    }

    public float X { get; set; }
    public float Y { get; set; }

    public static bool operator ==(CanvasPoint op1, CanvasPoint op2)
    {
        return op1.Equals(op2);
    }

    public static bool operator !=(CanvasPoint op1, CanvasPoint op2)
    {
        return !op1.Equals(op2);
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is CanvasPoint))
        {
            return false;
        }

        var p = (CanvasPoint)obj;

        return p.X == X && p.Y == Y;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
