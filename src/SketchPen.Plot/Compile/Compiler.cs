using SketchPen.Parse.Lexer;
using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SketchPen.Plot.Compile
{
    public class Compiler
    {
        static internal IEnumerable<Type> PlotCommandTypes = null;

        static Compiler()
        {
            PlotCommandTypes = Assembly.GetAssembly(typeof(Compiler))
                                       .GetTypes()
                                       .Where(t =>
                                                t.IsClass &&
                                                t.GetCustomAttribute<PlotCommandKeywordAttribute>() != null &&
                                                typeof(IPlotCommand).IsAssignableFrom(t));
        }

        public IEnumerable<IPlotCommand> Compile(string code, string customGlobalsName = "")
        {
            var syntax = new SketchPenSyntax();
            var lexicalAnalyser = new LexicalAnalyser(syntax);
            var tokens = lexicalAnalyser.Tokenize(code);

            var commands = tokens.GetStatements(syntax)
                                 .GetPlotCommands();

            return commands;
        }

        public string PreCompile(string fileName, string customGlobalsName = "")
        {
            var preCompiler = new PreComplier(fileName, customGlobalsName, true);
            string code = preCompiler.Compile(fileName);

            return code;
        }
    }
}
