using DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.DI
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ProductsDbContext>(options =>
                options.UseSqlite(connectionString));

            services.AddScoped<IProductRepository, ProductRepository>();

            return services;
        }
    }
}
