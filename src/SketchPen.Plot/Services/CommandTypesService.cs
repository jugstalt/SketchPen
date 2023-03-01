using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Compile;
using SketchPen.Plot.Models;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SketchPen.Plot.Services;

public class CommandTypesService
{
    private readonly Dictionary<Type, IEnumerable<EditorCompletionModel>> _editorCompletion;

    public CommandTypesService()
    {
        _editorCompletion = new Dictionary<Type, IEnumerable<EditorCompletionModel>>();

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

    public IEnumerable<Type> PlotCommandTypes => _editorCompletion.Keys;
    public IDictionary<Type, IEnumerable<EditorCompletionModel>> EditorCompletion => _editorCompletion;
}
