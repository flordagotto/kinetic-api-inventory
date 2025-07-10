namespace DAL.Repositories
{
    public interface IProductRepository
    {

    }

    public class ProductRepository : IProductRepository
    {
        private readonly ProductsDbContext _context;

        public ProductRepository(ProductsDbContext context)
        {
            _context = context;
        }
    }
}
