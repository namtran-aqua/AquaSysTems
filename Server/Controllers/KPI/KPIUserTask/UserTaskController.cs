
using AquaSolution.Server.Services.KPI.KPIUserTask;
using AquaSolution.Shared.KPI.KPIMonthlyTarget;
using AquaSolution.Shared.KPI.UserTask;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.KPI.KPIUserTask
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserTaskController : ControllerBase
    {
        private readonly IUserTaskService _userTaskService;

        public UserTaskController(IUserTaskService userTaskService)
        {
            _userTaskService = userTaskService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync([FromBody] HandleUserTaskAndTargetDto HandleUserTaskAndTarget)
        {
            var result = await _userTaskService.HandleUserTaskAndTarget(HandleUserTaskAndTarget);
            return result ? Ok(true) : BadRequest("New creation failed");
        }
        [HttpGet("list-taskIds/{userId}")]
        public async Task<IActionResult> GetActiveTaskIdsByUserId(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest("Invalid userId.");
            }

            var taskIds = await _userTaskService.GetListByUserIdAsync(userId);
            return Ok(taskIds);
        }
    }
}
