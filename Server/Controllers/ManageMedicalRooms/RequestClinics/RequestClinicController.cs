using AquaSolution.Server.Services.Administration.UserService;
using AquaSolution.Server.Services.ManageMedicalRooms.RequestClinicservice;
using AquaSolution.Shared.ManageMedicalRooms.RequestClinics;
using AquaSolution.Shared.ManageMedicalRooms.Treatments;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.ManageMedicalRooms.RequestClinics
{
    [ApiController]
    [Route("api/[controller]")]
    public class MyRequestClinicController : ControllerBase
    {
        private readonly IRequestClinicservice _RequestClinicservice;

        public MyRequestClinicController(IRequestClinicservice RequestClinicservice)
        {
            _RequestClinicservice = RequestClinicservice;
        }

        [HttpGet("get-reuqest-by-user")]
        public async Task<IActionResult> GetRequestByUser()
        {
            var result = await _RequestClinicservice.GetRequestByUser();
            return Ok(result);
        }
        [HttpPost("create-request")]
        public async Task<IActionResult> CreateRequest([FromBody] HandleMyRequestClinicDto requestDto)
        {
            if (requestDto == null)
            {
                return BadRequest("Request data is required.");
            }

            var result = await _RequestClinicservice.CreatedRequest(requestDto);

            if (result)
            {
                return Ok(new { Success = true, Message = "Request created successfully." });
            }

            return StatusCode(500, new { Success = false, Message = "Failed to create request." });
        }
        [HttpPost("approval")]
        public async Task<IActionResult> ApproveRequest([FromBody] UpdateRequestClinicStatusDto dto)
        {
            var result = await _RequestClinicservice.ApprovalAsync(dto.RequestClinicId, dto.UserId);
            if (result)
                return Ok(new { Success = true, Message = "Approved successfully." });

            return NotFound(new { Success = false, Message = "Approved unsuccessful " });
        }

        [HttpPost("reject")]
        public async Task<IActionResult> RejectRequest([FromBody] UpdateRequestClinicStatusDto dto)
        {
            var result = await _RequestClinicservice.RejectedAsync(dto.RequestClinicId, dto.UserId);
            if (result)
                return Ok(new { Success = true, Message = "Rejected successfully." });

            return NotFound(new { Success = false, Message = "Rejected unsuccessful." });
        }

        [HttpPost("done")]
        public async Task<IActionResult> MarkRequestAsDone([FromBody] UpdateRequestClinicStatusDto dto)
        {
            var result = await _RequestClinicservice.DoneAsync(dto.RequestClinicId, dto.UserId);
            if (result)
                return Ok(new { Success = true, Message = "Marked as done." });

            return NotFound(new { Success = false, Message = "Request unsuccessful" });
        }
        [HttpPost("created-treatment")]
        public async Task<IActionResult> CreatedTreatment([FromBody] CreatedTreatmentDto dto)
        {
            var result = await _RequestClinicservice.CreatedTreatment(dto);
            if (result)
                return Ok(new { Success = true, Message = "successfully" });

            return NotFound(new { Success = false, Message = "Error" });
        }

        [HttpGet("get-history/{requestId}")]
        public async Task<IActionResult> GetHistory(Guid requestId)
        {
            var result = await _RequestClinicservice.GetHistory(requestId)
                         ?? new MedicalHistoryDto(); // tạo object rỗng

            return Ok(result);
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _RequestClinicservice.DeleteMyRequestClinic(id);
            if (result)
                return Ok(new { success = true, message = "Deleted successfully" });

            return NotFound(new { success = false, message = "Delete operation failed" });
        }

    }
}
