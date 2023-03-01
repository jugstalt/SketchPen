namespace SketchPen.Plot.Services.Editor;

public class GlobalsFileEditorLanguageService : EditorLanguageService
{
    public GlobalsFileEditorLanguageService(CommandTypesService commandTypes)
        : base(commandTypes, EditorFileType.Globals) { }
}
