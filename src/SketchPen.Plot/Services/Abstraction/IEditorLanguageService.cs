using SketchPen.Plot.Models;
using System.Collections.Generic;

namespace SketchPen.Plot.Services.Abstraction;

public interface IEditorLanguageService
{
    bool MatchEditorFileType(EditorFileType fileType);

    IDictionary<string, IEnumerable<EditorCompletionModel>> EditorCompletion { get; }
}
