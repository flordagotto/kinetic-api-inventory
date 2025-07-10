using AutoMapper;
using DAL.Entities;
using DAL.Repositories;
using DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

namespace Services.Services
{
    public interface IProductService
    {
        Task Create(ProductInputDTO newProductDTO);
        Task<IEnumerable<ProductDTO>> GetAll();
        Task<ProductDTO?> GetById(Guid id);
        Task Update(Guid id, ProductInputDTO productDTO);
    }

    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository productRepository, IMapper mapper, ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task Create(ProductInputDTO newProductDTO)
        {
            try
            {
                var product = _mapper.Map<Product>(newProductDTO);

                await _productRepository.Add(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error creating a product.");
                throw;
            }
        }

        public async Task<IEnumerable<ProductDTO>> GetAll()
        {
            try
            {
                var products = await _productRepository.Get();

                if (products != null && products.Any())
                    return _mapper.Map<List<ProductDTO>>(products);

                return [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error retrieving products.");
                throw;
            }
        }

        public async Task<ProductDTO?> GetById(Guid id)
        {
            try
            {
                var product = await _productRepository.GetById(id);

                if (product != null)
                    return _mapper.Map<ProductDTO>(product);

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error retrieving product.");
                throw;
            }
        }

        public async Task Update(Guid id, ProductInputDTO productDTO)
        {
            try
            {
                var oldProduct = await _productRepository.GetById(id);

                if (oldProduct == null)
                    throw new ArgumentException($"Product with id {id} not found");
                // TODO: podria usar excepciones personalizadas y un middleware que trate esta excepcion como un 400

                oldProduct.Stock = productDTO.Stock;
                oldProduct.Price = productDTO.Price;
                oldProduct.Description = productDTO.Description;
                oldProduct.Category = productDTO.Category;
                oldProduct.ProductName = productDTO.ProductName;

                await _productRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error updating product.");
                throw;
            }
        }

        public async Task Delete(Guid id)
        {
            try
            {
                var product = await _productRepository.GetById(id);

                if (product == null)
                    throw new ArgumentException($"Product with id {id} not found");
                // TODO: podria usar excepciones personalizadas y un middleware que trate esta excepcion como un 400

                if (product != null)
                    await _productRepository.Delete(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error deleting product.");
                throw;
            }
        }

    }
}
