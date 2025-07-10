using AutoMapper;
using DAL.Entities;
using DAL.Repositories;
using DTOs;
using Microsoft.Extensions.Logging;

namespace Services.Services
{
    public interface IProductService
    {
        Task Create(NewProductDTO newProductDTO);
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

        public async Task Create(NewProductDTO newProductDTO)
        {
            try
            {
                var product = _mapper.Map<Product>(newProductDTO);

                await _productRepository.Add(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error creating a player.");
                throw;
            }
        }

    }
}
