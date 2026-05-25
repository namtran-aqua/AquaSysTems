
using AquaSolution.Server.Services.ScrapManagetment.MaterialServices;
using AquaSolution.Shared.ScrapManagement.Materials;
using Microsoft.AspNetCore.Mvc;


namespace AquaSolution.Server.Controllers.ScrapManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialController : ControllerBase
    {
        private readonly IMaterialService _materialService;

        public MaterialController(IMaterialService materialService)
        {
            _materialService = materialService;
        }

        [HttpPost("import-bom")]
        public async Task<IActionResult> ImportBOM([FromBody] List<ImportExcelDto> data)
        {
            try
            {
                if (data == null || !data.Any())
                {
                    return BadRequest(new { message = "Dữ liệu rỗng." });
                }

                var result = await _materialService.ImportMaterialsAsync(data);

                if (result)
                {
                    return Ok(new { message = $"Import thành công {data.Count} dòng." });
                }
                else
                {
                    return BadRequest(new { message = "Import thất bại." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        [HttpGet("get-all-materials")]
        public async Task<IActionResult> GetAllMaterials()
        {
            try
            {
                var result = await _materialService.GetMaterialsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        [HttpGet("get-scheduled-weights/{materialId}")]
        public async Task<IActionResult> GetScheduledWeights(Guid materialId)
        {
            try
            {
                var result = await _materialService.GetWeightByMaterial(materialId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }
        [HttpPost("update-weight")]
        public async Task<IActionResult> UpdateWeight([FromBody] UpdateWeightDto updateWeightDto)
        {
            try
            {
                if (updateWeightDto == null)
                {
                    return BadRequest("Dữ liệu cập nhật không hợp lệ.");
                }

                var result = await _materialService.UpdateMaterialWeightAsync(updateWeightDto);
                if (result)
                {
                    return Ok(new { message = "Cập nhật weight thành công." });
                }
                return BadRequest("Cập nhật weight thất bại.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }
    }
}
