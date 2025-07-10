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
        public async Task<IActionResult> Create([FromBody] ProductInputDTO newProductDTO)
        {
            await _productService.Create(newProductDTO);

            return Created();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _productService.GetAll();

            if (!result.Any())
                return NoContent();

            return Ok(result);
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetById([FromQuery] Guid id)
        {
            var result = await _productService.GetById(id);

            if (result == null)
                return NoContent();

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] ProductInputDTO productInputDTO)
        {
            await _productService.Update(id, productInputDTO);

            return Ok();
        }
    }
}
