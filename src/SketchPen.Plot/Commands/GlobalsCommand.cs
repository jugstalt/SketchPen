using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SketchPen.Plot.Commands
{
    [PlotCommandKeyword("globals")]
    [PlotCommandMethod("set", snippet: "set(${1:variableName}, ${2:value});")]
    [PlotCommandMethod("tryset", snippet: "tryset(${1:variableName}, ${2:value});")]
    class GlobalsCommand : GeneralPlotCommand
    {
        protected override void ExecuteCommand(IPlotContext context, IEnumerable<object> parameters)
        {
            switch (Method?.ToLower())
            {
                case "set":
                    if (parameters.Count() == 2)
                    {
                        context.Globals[parameters.Get<string>(0)] = parameters.Get<object>(1);
                    }
                    else
                    {
                        throw new Exception("Syntax error: declare.globals");
                    }
                    break;
                case "tryset":
                    if (parameters.Count() == 2)
                    {
                        if (!context.Globals.ContainsKey(parameters.Get<string>(0)))
                        {
                            context.Globals.Add(parameters.Get<string>(0),
                                                parameters.Get<object>(1));
                        }
                    }
                    else
                    {
                        throw new Exception("Syntax error: declare.globals");
                    }
                    break;
                default:
                    throw new Exception($"Unknown method: { Method  }");
            }
        }
    }
}
