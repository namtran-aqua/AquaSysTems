using AquaSolution.Server.Services.DepartmentService;
using AquaSolution.Shared.Departments;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.DepartmentManagement
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _departmentService.GetListDepartment();
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] DepartmentDto departmentDto)
        {
            var success = await _departmentService.CreatedDepartment(departmentDto);
            if (!success)
                return BadRequest("Failed to create department.");
            return Ok(true);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] DepartmentDto departmentDto)
        {
            var success = await _departmentService.UpdateDepartment(departmentDto);
            if (!success)
                return BadRequest("Failed to update department.");
            return Ok(true);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _departmentService.DeleteDepartment(id);
            if (!success)
                return BadRequest("Failed to delete department.");
            return Ok(true);
        }
    }
}
