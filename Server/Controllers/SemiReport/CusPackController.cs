using AquaSolution.Server.Services.SemiReport.CusPackService;
using AquaSolution.Shared.SemiReport;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.SemiReport
{
    [Route("api/[controller]")]
    [ApiController]
    public class CusPackController : ControllerBase
    {
        private readonly ICusPackService _cusPackService;
        public CusPackController(ICusPackService cusPackService)
        {
            _cusPackService = cusPackService;
        }
        // GET: api/CusPack
        [HttpGet("get-all")]
        public async Task<List<CusPackNoDto>> GetAll()
        {
            var result = await _cusPackService.GetAllAsync();
            if (result == null)
                return new List<CusPackNoDto>();
            return result;
        }
        // POST: api/CusPack
        [HttpPost("created")]
        public async Task<IActionResult> Create([FromBody] CusPackNoDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid data");
            var success = await _cusPackService.CreatedAsync(dto);
            if (success)
                return Ok(new { message = "Tạo thành công" });
            return StatusCode(500, "Tạo CusPack thất bại ");
        }
        // PUT: api/CusPack/{id}
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] CusPackNoDto dto)
        {
            var success = await _cusPackService.UpdateAsync(dto);
            if (success)
                return Ok(new { message = "Cập nhật thành công" });
            return NotFound("Không tìm thấy CusPack");
        }
        // DELETE: api/CusPack/{id}
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _cusPackService.DeleteAsync(id);
            if (success)
                return Ok(new { message = "Xóa thành công" });
            return NotFound("Không tìm thấy CusPack");
        }
    }
}
