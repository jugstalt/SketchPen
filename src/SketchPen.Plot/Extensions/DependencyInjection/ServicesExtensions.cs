using Microsoft.Extensions.DependencyInjection;
using SketchPen.Plot.Services;
using SketchPen.Plot.Services.Abstraction;
using SketchPen.Plot.Services.Editor;

namespace SketchPen.Plot.Extensions.DependencyInjection;

static public class ServicesExtensions
{
    static public IServiceCollection AddCompilerServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<CommandTypesService>()
            .AddTransient<CompilerService>();
    }

    static public IServiceCollection AddEditorLanguagerServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<IEditorLanguageService, CodeFileEditorLanguageService>()
            .AddSingleton<IEditorLanguageService, TempleteFileEditorLanguageService>()
            .AddSingleton<IEditorLanguageService, GlobalsFileEditorLanguageService>();
    }
}
