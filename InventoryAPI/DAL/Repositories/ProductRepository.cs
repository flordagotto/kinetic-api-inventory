using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public interface IProductRepository
    {
        Task Add(Product product);
        Task<IEnumerable<Product>> Get();
        Task<Product?> GetById(Guid id);
        Task Delete(Product product);
        Task Update();
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

        public async Task<IEnumerable<Product>> Get() => await _context.Products.ToListAsync();

        public async Task<Product?> GetById(Guid id) => await _context.Products.FirstOrDefaultAsync(x => x.Id == id);

        public async Task Delete(Product product)
        {
            _context.Products.Remove(product);

            await _context.SaveChangesAsync();
        }

        public async Task Update()
        {
            await _context.SaveChangesAsync();
        }
    }
}
