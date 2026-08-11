using Microsoft.Extensions.DependencyInjection;
using SketchPen.Commands;

namespace SketchPen.Extensions.DependencyInjection;

static public class ServiceCollectionExtensions
{
    static public IServiceCollection AddSketchPenCliServices(this IServiceCollection services)
        => services
            .AddTransient<RenderCommandHandler>()
            .AddTransient<ComposeCommandHandler>()
            .AddTransient<LanguageInfoCommandHandler>();
}
