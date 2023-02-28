using SketchPen.Parse.Lexer;
using System;
using System.Collections.Generic;

namespace SketchPen.Plot.Abstraction;

public interface IPlotCommand : IDisposable
{
    void SetStatement(string method, IEnumerable<object> parameters);

    void Init(IEnumerable<Token> statement);

    void Execute(IPlotContext context);
}
