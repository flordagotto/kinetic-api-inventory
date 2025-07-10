using Microsoft.Extensions.DependencyInjection;
using Services.Services;

namespace Services.DI
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductService>();
            services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

            return services;
        }
    }
}
