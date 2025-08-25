using AquaSolution.Server.Services.ManageMedicalRooms.InventoryPeriodService;
using AquaSolution.Server.Services.ManageMedicalRooms.MedicineSupplyRequestService;
using AquaSolution.Shared.ManageMedicalRooms.MedicineSupplyRequest;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.ManageMedicalRooms.MedicineSupplyRequest
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicineSupplyRequestController : ControllerBase
    {
        private readonly IMedicineSupplyRequestService _medicineSupplyRequestService;
        public MedicineSupplyRequestController(IMedicineSupplyRequestService medicineSupplyRequestService)
        {
            _medicineSupplyRequestService = medicineSupplyRequestService;
        }
        [HttpPost("create-medicine-supply-request")]
        public async Task<IActionResult> CreateAsync([FromBody] CreatedMedicineSupplyRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Request data is null.");

            var result = await _medicineSupplyRequestService.CreatedMedicineSupplyRequest(dto);

            if (!result)
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create medicine supply request.");

            return Ok(new { Success = true, Message = "Medicine supply request created successfully." });
        }
        [HttpGet("get-all-master")]
        public async Task<IActionResult> GetMasterMedicinSupplyRequest()
        {
            var result = await _medicineSupplyRequestService.GetListMedicineSuplyRequest();
            return Ok(result);
        }
        [HttpGet("get-detail-by-medicinsuplyrequestid/{Id}")]
        public async Task<IActionResult> GetMedicinSuplyrequestId(Guid id)
        {
            var result = await _medicineSupplyRequestService.GetListMedicineSuplyRequestDetail(id);
            return Ok(result);
        }

    }
}
