namespace SketchPen.Plot.Services.Editor;

public class TempleteFileEditorLanguageService : EditorLanguageService
{
    public TempleteFileEditorLanguageService(CommandTypesService commandTypes)
        : base(commandTypes, EditorFileType.Template) { }
}
