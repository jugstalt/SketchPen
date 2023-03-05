using SketchPen.Parse.Lexer;
using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Services;
using System.Collections.Generic;

namespace SketchPen.Plot.Compile;

public class Compiler
{
    private readonly CommandTypesService _commandTypes;

    public Compiler(CommandTypesService commandTypes)
    {
        _commandTypes = commandTypes;
    }

    public IEnumerable<IPlotCommand> Compile(string code, 
                                             string customGlobalsName = "")
    {
        var syntax = new SketchPenSyntax();
        var lexicalAnalyser = new LexicalAnalyser(syntax);
        var tokens = lexicalAnalyser.Tokenize(code);

        var commands = tokens.GetStatements(syntax)
                             .GetPlotCommands(_commandTypes);

        return commands;
    }

    public string PreCompile(string fileName, string customGlobalsName = "")
    {
        var preCompiler = new PreComplier(fileName, customGlobalsName, true);
        string code = preCompiler.Compile(fileName);

        return code;
    }
}
