using DAL.Repositories;

namespace Services.Services
{
    public interface IProductService
    {

    }

    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
    }
}
