using DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DAL
{
    public static class DependencyInjection
    {
        public static void AddInfrastructureServices(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ProductsDbContext>(options =>
                options.UseSqlite(connectionString));

            services.AddScoped<IProductRepository, ProductRepository>();
        }
    }
}
