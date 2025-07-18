﻿using Microsoft.Extensions.DependencyInjection;
using Services.Mappers;
using Services.Services;

namespace Services
{
    public static class DependencyInjection
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductService>();
            services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

            services.AddAutoMapper(typeof(ProductMapper).Assembly);
        }
    }
}
