using System;

namespace SketchPen.Plot;

public enum PenCap
{
    Round,
    Square,
    Flat
}

public enum EncodeFormat
{
    Png,
    Jpeg
}

[Flags]
public enum EditorFileType
{
    Unknown = 0,
    Globals = 1,
    Template = 2,
    Code = 4
}
