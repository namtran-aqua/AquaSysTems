using AquaSolution.Server.Services.ManageMedicalRooms.Products;
using AquaSolution.Shared.ManageMedicalRooms.Products;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.ManageMedicalRooms.ProductManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("get-all-by-import")]
        public async Task<IActionResult> GetAllByImport()
        {
            var result = await _productService.GetListProductByImport();
            return Ok(result);
        }
        [HttpGet("get-all-by-export")]
        public async Task<IActionResult> GetAllByExport()
        {
            var result = await _productService.GetListProductByExport();
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ProductDto productDto)
        {
            var success = await _productService.CreatedProduct(productDto);
            if (!success)
                return BadRequest("Failed to create product.");
            return Ok(true);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] ProductDto productDto)
        {
            var success = await _productService.UpdateProduct(productDto);
            if (!success)
                return BadRequest("Failed to update product.");
            return Ok(true);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _productService.DeleteProduct(id);
            if (!success)
                return BadRequest("Failed to delete product.");
            return Ok(true);
        }
    }
}
