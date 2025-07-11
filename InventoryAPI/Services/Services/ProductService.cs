using AutoMapper;
using DAL.Entities;
using DAL.Repositories;
using DTOs.ApiDtos;
using DTOs.RabbitDtos;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System.Text.Json;

namespace Services.Services
{
    public interface IProductService
    {
        Task Create(ProductInputDTO newProductDTO);
        Task<IEnumerable<ProductDTO>> GetAll();
        Task<ProductDTO?> GetById(Guid id);
        Task Update(Guid id, ProductInputDTO productDTO);
        Task Delete(Guid id);
    }

    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;
        private readonly IRabbitMqPublisher _rabbitPublisher;

        public ProductService(IProductRepository productRepository, IMapper mapper, ILogger<ProductService> logger, IRabbitMqPublisher rabbitPublisher)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _logger = logger;
            _rabbitPublisher = rabbitPublisher;
        }

        public async Task Create(ProductInputDTO newProductDTO)
        {
            try
            {
                var product = _mapper.Map<Product>(newProductDTO);

                await _productRepository.Add(product);

                var eventMessage = CreateEventMessage(product, ProductEventType.Created);

                await _rabbitPublisher.PublishAsync(eventMessage);
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
                var product = await _productRepository.GetById(id);

                if (product == null)
                    throw new ArgumentException($"Product with id {id} not found");
                // TODO: podria usar excepciones personalizadas y un middleware que trate esta excepcion como un 400

                product.Stock = productDTO.Stock;
                product.Price = productDTO.Price;
                product.Description = productDTO.Description;
                product.Category = productDTO.Category;
                product.ProductName = productDTO.ProductName;

                await _productRepository.SaveChangesAsync();

                var eventMessage = CreateEventMessage(product, ProductEventType.Updated);

                await _rabbitPublisher.PublishAsync(eventMessage);
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
                {
                    await _productRepository.Delete(product);

                    var eventMessage = CreateEventMessage(product, ProductEventType.Deleted);

                    await _rabbitPublisher.PublishAsync(eventMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Error deleting product.");
                throw;
            }
        }

        private EventMessage CreateEventMessage(Product product, ProductEventType eventType) =>
            new()
            {
                ProductId = product.Id,
                EventDate = DateTime.Now,
                EventType = eventType,
            };
    }
}
