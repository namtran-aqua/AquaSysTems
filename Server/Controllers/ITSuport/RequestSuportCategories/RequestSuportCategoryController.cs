using AquaSolution.Server.Services.ITSuport.RequestSuportCategories;
using AquaSolution.Shared.ITSuport.RequestSuportCategory;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.ITSuport.RequestSuportCategories
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestSuportCategoryController : ControllerBase
    {
        private readonly IRequestSuportCategoryService _service;

        public RequestSuportCategoryController(IRequestSuportCategoryService service)
        {
            _service = service;
        }

        // GET: api/RequestSuportCategory
        [HttpGet("get-all")]
        public async Task<List<RequestSuportCategoryDto>> GetAll()
        {
            var result = await _service.GetAllAsync();
            if (result == null)
                return new List<RequestSuportCategoryDto>();
            return result;
        }

        // POST: api/RequestSuportCategory
        [HttpPost("created")]
        public async Task<IActionResult> Create([FromBody] RequestSuportCategoryDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid data");

            var success = await _service.CreatedAsync(dto);
            if (success)
                return Ok(new { message = "Created successfully" });

            return StatusCode(500, "Error creating category");
        }

        // PUT: api/RequestSuportCategory/{id}
        [HttpPut("update")]
        public async Task<IActionResult> Update( [FromBody] RequestSuportCategoryDto dto)
        {
     
            var success = await _service.UpdateAsync(dto);
            if (success)
                return Ok(new { message = "Updated successfully" });

            return NotFound("Category not found");
        }

        // DELETE: api/RequestSuportCategory/{id}
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAssync(id);
            if (success)
                return Ok(new { message = "Deleted successfully" });

            return NotFound("Category not found");
        }
    }
}
