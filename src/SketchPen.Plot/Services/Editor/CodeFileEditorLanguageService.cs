namespace SketchPen.Plot.Services.Editor;

public class CodeFileEditorLanguageService : EditorLanguageService
{
    public CodeFileEditorLanguageService(CommandTypesService commandTypes)
        : base(commandTypes, EditorFileType.Code) 
    {
    }
}
