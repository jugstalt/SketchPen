using SketchPen.Plot.Models;

namespace SketchPen.Code.Models.Code
{
    public class EditFileModel
    {
        public string Route { get; set; } = String.Empty;
        public string Content { get; set; } = String.Empty;

        public IDictionary<string, IEnumerable<EditorCompletionModel>>? EditorCompletion { get; set; }
    }
}
