using AutoMapper;
using DAL.Entities;
using DAL.Repositories;
using DTOs.ApiDtos;
using DTOs.RabbitDtos;
using FluentAssertions;
using Moq;
using Moq.AutoMock;
using Services.Mappers;
using Services.Services;

namespace UnitTests
{
    public class ProductServiceTests
    {
        AutoMocker _mocker;

        IProductService _service;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ProductMapper());
            });

            var realMapper = configuration.CreateMapper();

            _mocker.Use<IMapper>(realMapper);

            _service = _mocker.CreateInstance<ProductService>();
        }

        [Test]
        public async Task Create_HappyPath()
        {
            var productDto = new ProductInputDTO { ProductName = "Jamon", Category = "Alimento", Description = "Jamon LA PAULINA 500gr", Price = 7500, Stock = 15 };

            await _service.Create(productDto);

            _mocker.GetMock<IProductRepository>().Verify(x => x.Add(It.IsAny<Product>()));
            _mocker.GetMock<IRabbitMqPublisher>().Verify(x => x.PublishAsync(It.IsAny<EventMessage>()), Times.Once);
        }

        [Test]
        public async Task Update_HappyPath()
        {
            var id = Guid.NewGuid();
            var productDto = new ProductInputDTO { ProductName = "Jamon", Category = "Alimento", Description = "Jamon LA PAULINA 500gr", Price = 7500, Stock = 15 };
            var product = new Product { Id = id, ProductName = "Jamon", Category = "Alimento", Description = "Jamon LA PAULINA 500gr", Price = 7500, Stock = 15 };

            _mocker.GetMock<IProductRepository>()
                .Setup(x => x.GetById(id))
                .ReturnsAsync(product);

            await _service.Update(id, productDto);

            _mocker.GetMock<IProductRepository>().Verify(x => x.Update(), Times.Once);
            _mocker.GetMock<IRabbitMqPublisher>().Verify(x => x.PublishAsync(It.IsAny<EventMessage>()), Times.Once);
        }

        [Test]
        public async Task Update_WhenTheProductIsNotFound_ShouldThrowArgumentException()
        {
            var id = Guid.NewGuid();
            var productDto = new ProductInputDTO { ProductName = "Jamon", Category = "Alimento", Description = "Jamon LA PAULINA 500gr", Price = 7500, Stock = 15 };
            var product = new Product { Id = id, ProductName = "Jamon", Category = "Alimento", Description = "Jamon LA PAULINA 500gr", Price = 7500, Stock = 15 };

            _mocker.GetMock<IProductRepository>()
                .Setup(x => x.GetById(id))
                .ThrowsAsync(new ArgumentException());

            var act = async () => await _service.Update(id, productDto);

            await act.Should().ThrowAsync<ArgumentException>($"Product with id {id} not found");

            _mocker.GetMock<IProductRepository>().Verify(x => x.Update(), Times.Never);
            _mocker.GetMock<IRabbitMqPublisher>().Verify(x => x.PublishAsync(It.IsAny<EventMessage>()), Times.Never);
        }

        [Test]
        public async Task GetAll_HappyPath()
        {
            var product1 = new Product { Id = Guid.NewGuid(), ProductName = "Jamon", Category = "Alimento", Description = "Jamon LA PAULINA 500gr", Price = 7500, Stock = 15 };
            var product2 = new Product { Id = Guid.NewGuid(), ProductName = "Queso", Category = "Alimento", Description = "Queso LA PAULINA 500gr", Price = 10200, Stock = 10 };

            var products = new List<Product> { product1, product2 };

            _mocker.GetMock<IProductRepository>().Setup(x => x.Get()).ReturnsAsync(products);

            var productDtos = await _service.GetAll();

            productDtos.Should().NotBeNull();

            var product1Dto = productDtos.First();
            product1Dto.Id.Should().Be(product1.Id);
            product1Dto.ProductName.Should().Be(product1.ProductName);
            product1Dto.Description.Should().Be(product1.Description);
            product1Dto.Category.Should().Be(product1.Category);
            product1Dto.Price.Should().Be(product1.Price);
            product1Dto.Stock.Should().Be(product1.Stock);

            _mocker.GetMock<IProductRepository>().Verify(x => x.Get(), Times.Once);
            _mocker.GetMock<IRabbitMqPublisher>().Verify(x => x.PublishAsync(It.IsAny<EventMessage>()), Times.Never);
        }

        [Test]
        public async Task GetById_HappyPath()
        {
            var product1 = new Product { Id = Guid.NewGuid(), ProductName = "Jamon", Category = "Alimento", Description = "Jamon LA PAULINA 500gr", Price = 7500, Stock = 15 };
            var product2 = new Product { Id = Guid.NewGuid(), ProductName = "Queso", Category = "Alimento", Description = "Queso LA PAULINA 500gr", Price = 10200, Stock = 10 };

            _mocker.GetMock<IProductRepository>().Setup(x => x.GetById(product2.Id)).ReturnsAsync(product2);

            var productDto = await _service.GetById(product2.Id);

            productDto.Should().NotBeNull();

            productDto.Id.Should().Be(product2.Id);
            productDto.ProductName.Should().Be(product2.ProductName);
            productDto.Description.Should().Be(product2.Description);
            productDto.Category.Should().Be(product2.Category);
            productDto.Price.Should().Be(product2.Price);
            productDto.Stock.Should().Be(product2.Stock);

            _mocker.GetMock<IProductRepository>().Verify(x => x.GetById(product2.Id), Times.Once);
            _mocker.GetMock<IRabbitMqPublisher>().Verify(x => x.PublishAsync(It.IsAny<EventMessage>()), Times.Never);
        }

        [Test]
        public async Task Delete_HappyPath()
        {
            var id = Guid.NewGuid();
            
            var product = new Product { Id = id, ProductName = "Jamon", Category = "Alimento", Description = "Jamon LA PAULINA 500gr", Price = 7500, Stock = 15 };

            _mocker.GetMock<IProductRepository>()
                .Setup(x => x.GetById(id))
                .ReturnsAsync(product);

            await _service.Delete(id);

            _mocker.GetMock<IProductRepository>().Verify(x => x.Delete(product), Times.Once);
            _mocker.GetMock<IRabbitMqPublisher>().Verify(x => x.PublishAsync(It.IsAny<EventMessage>()), Times.Once);
        }

        [Test]
        public async Task Delete_WhenTheProductIsNotFound_ShouldThrowArgumentException()
        {
            var id = Guid.NewGuid();
            
            var product = new Product { Id = id, ProductName = "Jamon", Category = "Alimento", Description = "Jamon LA PAULINA 500gr", Price = 7500, Stock = 15 };

            _mocker.GetMock<IProductRepository>()
                .Setup(x => x.GetById(id))
                .ThrowsAsync(new ArgumentException());

            var act = async () => await _service.Delete(id);

            await act.Should().ThrowAsync<ArgumentException>($"Product with id {id} not found");

            _mocker.GetMock<IProductRepository>().Verify(x => x.Update(), Times.Never);
            _mocker.GetMock<IRabbitMqPublisher>().Verify(x => x.PublishAsync(It.IsAny<EventMessage>()), Times.Never);
        }

    }
}