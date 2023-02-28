using SketchPen.Parse.Lexer;
using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Extensions;
using SketchPen.Plot.Models;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SketchPen.Plot.Compile;

public class Compiler
{
    static readonly Dictionary<Type, IEnumerable<EditorCompletionModel>> _editorCompletion = new Dictionary<Type, IEnumerable<EditorCompletionModel>>();

    #region Static members

    static Compiler()
    {
        var plotCommandTypes = Assembly.GetAssembly(typeof(Compiler))
                                       .GetTypes()
                                       .Where(t =>
                                              t.IsClass &&
                                              t.GetCustomAttribute<PlotCommandKeywordAttribute>() != null &&
                                              typeof(IPlotCommand).IsAssignableFrom(t));

        //_editorCompletion.Add("#include", Array.Empty<EditorCompletionModel>());

        foreach (var plotCommandType in plotCommandTypes)
        {
            var keywordAttribute = plotCommandType.GetCustomAttribute<PlotCommandKeywordAttribute>();

            _editorCompletion.Add(plotCommandType,
                                  plotCommandType.GetCustomAttributes<PlotCommandMethodAttribute>()
                                                 .Select(a => new EditorCompletionModel(a.Name, a.Suggestion, a.Snippet)));
        }
    }

    static internal IEnumerable<Type> PlotCommandTypes => _editorCompletion.Keys;
    static internal IDictionary<Type, IEnumerable<EditorCompletionModel>> EditorCompletion => _editorCompletion;

    #endregion

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
