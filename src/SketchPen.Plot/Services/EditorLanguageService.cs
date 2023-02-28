using SketchPen.Plot.Compile;
using SketchPen.Plot.Models;
using SketchPen.Plot.Reflection;
using SketchPen.Plot.Services.Abstraction;
using System.Collections.Generic;
using System.Reflection;

namespace SketchPen.Plot.Services;

public class EditorLanguageService : IEditorLanguageService
{
    private readonly IDictionary<string, IEnumerable<EditorCompletionModel>> _editorCompletion;
    private readonly EditorFileType _editorFileType;

    public EditorLanguageService(EditorFileType fileType)
    {
        var compiler = new Compiler(); // create instance => static constructor

        _editorCompletion = new Dictionary<string, IEnumerable<EditorCompletionModel>>();
        _editorFileType = fileType;

        foreach (var plotCommandType in Compiler.EditorCompletion.Keys)
        {
            var fileTypeAttribute = plotCommandType.GetCustomAttribute<PlotCommandSupportedFileTypesAttribute>();

            if (fileTypeAttribute == null || !fileTypeAttribute.EditorFileTypes.HasFlag(fileType))
            {
                continue;
            }

            var keywordAttribute = plotCommandType.GetCustomAttribute<PlotCommandKeywordAttribute>();

            if (keywordAttribute == null)
            {
                continue;
            }

            _editorCompletion.TryAdd(keywordAttribute.Keyword,
                                     new List<EditorCompletionModel>(Compiler.EditorCompletion[plotCommandType]));
        }
    }

    public IDictionary<string, IEnumerable<EditorCompletionModel>> EditorCompletion
        => _editorCompletion;

    public bool MatchEditorFileType(EditorFileType fileType)
    {
        return fileType == _editorFileType;
    }
}
