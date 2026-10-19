using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace RS.MVC.Features
{
    /// <summary>
    /// The app's use cases (commands and queries) live under Features/, one file each, with a handler class named
    /// …Handler. Controllers inject the handlers they need directly; there is no dispatcher.
    /// </summary>
    public static class FeatureRegistration
    {
        /// <summary>Registers every …Handler class under RS.MVC.Features as scoped (one per request).</summary>
        public static IServiceCollection AddFeatureHandlers(this IServiceCollection services)
        {
            var handlers = typeof(FeatureRegistration).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Handler")
                    && t.Namespace != null && t.Namespace.StartsWith(typeof(FeatureRegistration).Namespace!));
            foreach (var handler in handlers)
            {
                services.AddScoped(handler);
            }
            return services;
        }
    }
}
