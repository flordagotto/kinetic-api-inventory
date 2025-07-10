using DAL.Repositories;

namespace Services.Services
{
    public interface IProductService
    {

    }

    public class ProductService : IProductService
    {
        private readonly ProductRepository _productRepository;

        public ProductService(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
    }
}
