using Microsoft.Extensions.DependencyInjection;
using SketchPen.Plot.Services;

namespace SketchPen.Plot.Extensions.DependencyInjection
{
    static public class ServicesExtensions
    {
        static public IServiceCollection AddCompilerService(this IServiceCollection services)
        {
            return services.AddSingleton<CompilerService>();
        }
    }
}
