using SketchPen.Parse.Lexer;
using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SketchPen.Plot.Commands;

abstract class GeneralPlotCommand : IPlotCommand
{
    private string _method = String.Empty;
    private IEnumerable<object> _parameters = Array.Empty<object>();

    private IEnumerable<Token> _statement = Array.Empty<Token>();

    protected string Method => _method;
    protected IEnumerable<object> Parameters => _parameters;

    #region IDisposable

    virtual public void Dispose()
    {

    }

    #endregion

    abstract protected void ExecuteCommand(IPlotContext context, IEnumerable<object> parameters);

    #region IPlotCommand

    virtual public void Init(IEnumerable<Token> statement)
    {
        _statement = statement;
    }

    public void Execute(IPlotContext context)
    {
        try
        {
            ExecuteCommand(context, parameters: Parameters?.Select(p =>
            {
                if (p != null && p.ToString().StartsWith("@@"))
                {
                    var variableName = p.ToString().Substring(2);
                    if (!context.Globals.ContainsKey(variableName))
                    {
                        throw new SyntaxErrorException($"Unknown variable: {variableName}", _statement);
                    }

                    return context.Globals[variableName];
                }
                else
                {
                    return p;
                }
            }) ?? Array.Empty<object>());
        }
        catch (SyntaxErrorException see)
        {
            throw see;
        }
        catch (Exception ex)
        {
            throw new SyntaxErrorException(ex.Message, _statement);
        }
    }

    public void SetStatement(string method, IEnumerable<object> parameters)
    {
        _method = method;
        _parameters = parameters;
    }

    #endregion
}
