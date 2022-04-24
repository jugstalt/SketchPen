using SketchPen.Code.Services;

namespace SketchPen.Code.Extensions.DependencyInjection
{
    static public class ServiceCollectionExtensions
    {
        static public IServiceCollection AddSketchPenCodeServices(this IServiceCollection services,
                                                                 Action<SketchPenCodeServiceOptions> setupAction)
        {
            services.Configure(setupAction);

            return services.AddTransient<SketchPenCodeService>()
                           .AddTransient<SketchPenPlotService>();
        }
    }
}
