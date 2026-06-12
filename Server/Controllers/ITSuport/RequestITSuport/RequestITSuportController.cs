using AquaSolution.Server.Services.ITSuport.RequestSuportCategories;
using AquaSolution.Server.SignalR;
using AquaSolution.Shared.ITSuport.Attachments;
using AquaSolution.Shared.ITSuport.RequestSuport;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AquaSolution.Server.Controllers.ITSuport.RequestSuportCategories
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestITSuportController : ControllerBase
    {
        private readonly IRequestITSuportService _service;
        private readonly IHubContext<SignalrHub> _hubContext;
        public RequestITSuportController(IRequestITSuportService service,
            IHubContext<SignalrHub> hubContext)
        {
            _service = service;
            _hubContext = hubContext;
        }
        [HttpGet("get-paged")]
        public async Task<IActionResult> GetPaged([FromQuery] RequestSuportQueryDto request)
        {
            var result = await _service.GetPagedAsync(request);
            return Ok(result);
        }
        [HttpGet("get-all")]
        public async Task<List<RequestSuportDto>> GetAll()
        {
            var result = await _service.GetAllAsync();
            if (result == null)
                return new List<RequestSuportDto>();
            return result;
        }
        [HttpGet("get-attechment/{id}")]
        public async Task<List<AttachmentDto>> GetAllAttechment(Guid id)
        {
            var result = await _service.LoadListAttachment(id);
            if (result == null)
                return new List<AttachmentDto>();
            return result;
        }

        // POST: api/RequestSuportCategory
        [HttpPost("created")]
        public async Task<IActionResult> Create([FromBody] HandleRequestSuportDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid data");

            var success = await _service.CreatedAsync(dto);
            if (success)
            {
                await _hubContext.Clients.All.SendAsync("LoadRequestSuport");
                return Ok(new { message = "Created successfully" });
            }

            return StatusCode(500, "Error creating category");
        }

        [HttpGet("get-stats")]
        public async Task<IActionResult> GetStats(
                [FromQuery] Guid? currentUserId,
                [FromQuery] bool isITOrAdmin)
        {
            var result = await _service.GetStatsAsync(currentUserId, isITOrAdmin);
            return Ok(result);
        }
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] HandleRequestSuportDto dto)
        {
            var success = await _service.UpdateAsync(dto);
            if (success)
            {
                await _hubContext.Clients.All.SendAsync("LoadRequestSuport");
                return Ok(new { message = "Updated successfully" });

            }


            return NotFound("Category not found");
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAssync(id);
            if (success)
            {
                await _hubContext.Clients.All.SendAsync("LoadRequestSuport");
                return Ok(new { message = "Deleted successfully" });
            }

            return NotFound("Category not found");
        }
    }
}
