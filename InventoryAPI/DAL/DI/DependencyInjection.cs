using DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.DI
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddDbContext<ProductsDbContext>(options =>
                options.UseSqlite("Data Source=notifications.db"));

            services.AddScoped<IProductRepository, ProductRepository>();

            return services;
        }
    }
}
