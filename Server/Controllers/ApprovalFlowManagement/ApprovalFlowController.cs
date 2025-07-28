using AquaSolution.Server.Services.ApprovalFlowService;
using AquaSolution.Shared.ApprovalFlows;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.ApprovalFlowManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApprovalFlowController : ControllerBase
    {
        private readonly IApprovalFlowService _approvalFlowService;

        public ApprovalFlowController(IApprovalFlowService approvalFlowService)
        {
            _approvalFlowService = approvalFlowService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _approvalFlowService.GetListApprovalFlow();
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ApprovalFlowDto approvalFlowDto)
        {
            var success = await _approvalFlowService.CreatedApprovalFlow(approvalFlowDto);
            if (!success)
                return BadRequest("Failed to create approvalFlow.");
            return Ok(true);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] ApprovalFlowDto approvalFlowDto)
        {
            var success = await _approvalFlowService.UpdateApprovalFlow(approvalFlowDto);
            if (!success)
                return BadRequest("Failed to update approvalFlow.");
            return Ok(true);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _approvalFlowService.DeleteApprovalFlow(id);
            if (!success)
                return BadRequest("Failed to delete approvalFlow.");
            return Ok(true);
        }
    }
}
