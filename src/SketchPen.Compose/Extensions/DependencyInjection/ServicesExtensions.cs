using Microsoft.Extensions.DependencyInjection;
using SketchPen.Compose.Services;
using SketchPen.Compose.Services.Absraction;
using SketchPen.Plot.Abstraction;
using System;

namespace SketchPen.Compose.Extensions.DependencyInjection;

static public class ServicesExtensions
{
    static public IServiceCollection AddComposerServices<T>(this IServiceCollection services)
        where T : IPlotContext
    {
        Action<ComposeHelperServiceOptions> setupAction = (options) =>
        {
            options.PlotContextType = typeof(T);
        };

        services.Configure<ComposeHelperServiceOptions>(setupAction);

        return services
                .AddTransient<ComposeHelperService>()
                .AddTransient<IComposerService, SingleImageComposerService>()
                .AddTransient<IComposerService, ImagesComposerService>()
                .AddTransient<IComposerService, WebSpriteComposerService>();
    }
}
