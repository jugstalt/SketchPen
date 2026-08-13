using SketchPen.Plot.Abstraction;
using System;
using System.Collections.Generic;

namespace SketchPen.Plot.Debugging;

public class DebugPlotContext : IPlotContext
{
    private readonly IDictionary<string, object> _globals;
    private readonly IPlotContext? _context;
    private ICanvas _canvas;
    private IPlotPath? _currentPath;

    public DebugPlotContext()
    {
        _globals = new Dictionary<string, object>();
        _canvas = new DebugCanvas();
    }

    public DebugPlotContext(Type plotContextType)
    {
        _globals = new Dictionary<string, object>();
        _canvas = new DebugCanvas();

        _context = Activator.CreateInstance(plotContextType) as IPlotContext;
        _canvas = new DebugCanvas(_context?.Canvas);
    }

    public ICanvas Canvas => _canvas;

    public PlotColor PenColor
    {
        set
        {
            if (_context != null)
            {
                _context.PenColor = value;
            }
        }
    }

    public float PenWidth
    {
        set
        {
            if (_context != null)
            {
                _context.PenWidth = value;
            }
        }
    }

    public PenCap PenCap
    {
        set
        {
            if (_context != null)
            {
                _context.PenCap = value;
            }
        }
    }

    public float MaxPenWidth
    {
        set
        {
            if (_context != null)
            {
                _context.MaxPenWidth = value;
            }
        }
    }

    public float MinPenWidth
    {
        set
        {
            if (_context != null)
            {
                _context.MinPenWidth = value;
            }
        }
    }

    public PlotColor BrushColor
    {
        set
        {
            if (_context != null)
            {
                _context.BrushColor = value;
            }
        }
    }

    public PlotColor GradientBrushColor
    {
        set
        {
            if (_context != null)
            {
                _context.GradientBrushColor = value;
            }
        }
    }

    public CanvasPoint GradientBrushPoint1
    {
        set
        {
            if (_context != null)
            {
                _context.GradientBrushPoint1 = value;
            }
        }
    }

    public CanvasPoint GradientBrushPoint2
    {
        set
        {
            if (_context != null)
            {
                _context.GradientBrushPoint2 = value;
            }
        }
    }

    public IDictionary<string, object> Globals 
        => _context?.Globals ?? _globals;

    public IBrush CreateBrush(IEnumerable<object> parameters)
        => _context?.CreateBrush(parameters) ?? new DebugBrush();

    public IFont CreateFont(IEnumerable<object> parameters)
        => _context?.CreateFont(parameters) ?? new DebugFont();

    public IPen CreatePen(IEnumerable<object> parameters)
        => _context?.CreatePen(parameters) ?? new DebugPen();


    public IPlotPath CreatePlotPath()
        => _context?.CreatePlotPath() ?? new DebugPlotPath();

    public IPlotPath? CurrentPath
    {
        get => _context != null ? _context.CurrentPath : _currentPath;
        set
        {
            if (_context != null)
            {
                _context.CurrentPath = value;
            }
            else
            {
                _currentPath = value;
            }
        }
    }


    public void Dispose()
    {
        _context?.Dispose();
    }

    public byte[] Encode(EncodeFormat format)
        => _context?.Encode(format) ?? Array.Empty<byte>();


    public void Init(int width, int height, PlotContextOrigin origin = PlotContextOrigin.Center)
    {
        _context?.Init(width, height, origin);

        _canvas = new DebugCanvas(_context?.Canvas);
    }

    public CanvasPoint Project(CanvasPoint point)
        => _context?.Project(point) ?? point;


    public CanvasRectangle Project(CanvasRectangle rect)
        => _context?.Project(rect) ?? rect;


    public float Project(float number)
        => _context?.Project(number) ?? number;


    public void ResetTransform()
    {
        _context?.ResetTransform();
    }
}
