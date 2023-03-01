using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Compile;
using System;
using System.Collections.Generic;
using System.Text;

namespace SketchPen.Plot.Services;

public class CompilerService
{
    private readonly Compiler _compiler;

    public CompilerService(CommandTypesService commandTypes)
    {
        _compiler = new Compiler(commandTypes);
    }

    public IEnumerable<IPlotCommand> Compile(string code,
                                             string customGlobalsName = "") => _compiler.Compile(code, customGlobalsName);

    public string PreCompile(string fileName, 
                             string customGlobalsName = "") => _compiler.PreCompile(fileName, customGlobalsName);
}
