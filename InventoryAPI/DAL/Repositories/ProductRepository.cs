using DAL.Entities;
using System.Numerics;

namespace DAL.Repositories
{
    public interface IProductRepository
    {
        Task Add(Product product);
    }

    public class ProductRepository : IProductRepository
    {
        private readonly ProductsDbContext _context;

        public ProductRepository(ProductsDbContext context)
        {
            _context = context;
        }

        public async Task Add(Product product)
        {
            _context.Products.Add(product);

            await _context.SaveChangesAsync();
        }
    }
}
