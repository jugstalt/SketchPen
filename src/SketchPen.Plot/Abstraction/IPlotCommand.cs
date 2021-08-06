using SketchPen.Parse.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace SketchPen.Plot.Abstraction
{
    public interface IPlotCommand : IDisposable
    {
        void SetStatement(string method, IEnumerable<object> parameters);

        void Execute(IPlotContext context);
    }
}
