using MVCApplication.Handlers;

namespace MVCApplication.Extensions
{
    public static class HttpClientExtensions
    {
        // Public Client (No attach JWT)
        public static IServiceCollection AddGatewayPublicClient<TInterface, TImplementation>(
            this IServiceCollection services,
            string baseUrl)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.AddHttpClient<TInterface, TImplementation>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
            });

            return services;
        }

        // Auth Client (Attach JWT)
        public static IServiceCollection AddGatewayAuthClient<TInterface, TImplementation>(
            this IServiceCollection services,
            string baseUrl)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.AddHttpContextAccessor();
            services.AddTransient<JwtDelegatingHandler>();

            services.AddHttpClient<TInterface, TImplementation>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
            })
            .AddHttpMessageHandler<JwtDelegatingHandler>();

            return services;
        }
    }
}
