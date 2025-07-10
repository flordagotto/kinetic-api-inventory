using DTOs;
using Microsoft.AspNetCore.Mvc;
using Services.Services;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NewProductDTO newProductDTO)
        {
            await _productService.Create(newProductDTO);

            return Created();
        }

    }
}
