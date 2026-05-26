using AquaSolution.Server.Services.ScrapManagetment.FlowApprovalServices;
using AquaSolution.Shared.ScrapManagement.FlowApprovals;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.ScrapManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlowApprovalScrapController : ControllerBase
    {
        private readonly IFlowApprovalService _flowApprovalService;

        public FlowApprovalScrapController(IFlowApprovalService flowApprovalService)
        {
            _flowApprovalService = flowApprovalService;
        }

        [HttpGet("get-by-department/{departmentId}/{factoryId}")]
        public async Task<IActionResult> GetFlowApproval(Guid departmentId, Guid factoryId)
        {
            try
            {
                var result = await _flowApprovalService.GetFlowApprovalAsync(departmentId, factoryId);
                if (result == null)
                    return NotFound(new { message = "Không tìm thấy luồng duyệt" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        [HttpGet("get-all-by-factory/{factoryId}")]
        public async Task<IActionResult> GetAllByFactory(Guid factoryId)
        {
            try
            {
                var result = await _flowApprovalService.GetAllByFactoryAsync(factoryId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateFlowApproval([FromBody] CreateFlowApprovalRequest request)
        {
            try
            {
                var result = await _flowApprovalService.CreateFlowApprovalAsync(request);
                if (!result)
                    return BadRequest(new { message = "Luồng duyệt đã tồn tại hoặc tạo thất bại" });

                return Ok(new { message = "Tạo luồng duyệt thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        [HttpPut("update-step/{id}")]
        public async Task<IActionResult> UpdateFlowStep(Guid id, [FromBody] UpdateFlowStepRequest request)
        {
            try
            {
                var result = await _flowApprovalService.UpdateFlowStepAsync(id, request);
                if (!result)
                    return NotFound(new { message = "Không tìm thấy step cần update" });

                return Ok(new { message = "Cập nhật step thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        [HttpDelete("delete-step/{id}")]
        public async Task<IActionResult> DeleteFlowStep(Guid id)
        {
            try
            {
                var result = await _flowApprovalService.DeleteFlowStepAsync(id);
                if (!result)
                    return NotFound(new { message = "Không tìm thấy step cần xóa" });

                return Ok(new { message = "Xóa step thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        [HttpDelete("delete-flow/{departmentId}/{factoryId}")]
        public async Task<IActionResult> DeleteFlowApproval(Guid departmentId, Guid factoryId)
        {
            try
            {
                var result = await _flowApprovalService.DeleteFlowApprovalAsync(departmentId, factoryId);
                if (!result)
                    return NotFound(new { message = "Không tìm thấy luồng duyệt cần xóa" });

                return Ok(new { message = "Xóa luồng duyệt thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _flowApprovalService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }
    }
}