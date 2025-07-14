using DTOs.ApiDtos;
using Microsoft.AspNetCore.Mvc;
using Services.Services;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController(IProductService productService) : ControllerBase
    {
        private readonly IProductService _productService = productService;

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            await _productService.Delete(id);

            return Ok();
        }
    }
}
