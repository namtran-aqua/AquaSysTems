using AquaSolution.Server.Services.Administration.SectionService;
using AquaSolution.Shared.Administration.Sections;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AquaSolution.Server.Controllers.Administration.SectionManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class SectionController : ControllerBase
    {
        private readonly ISectionService _sectionService;

        public SectionController(ISectionService sectionService)
        {
            _sectionService = sectionService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetSections()
        {
            var sections = await _sectionService.GetListSection();
            return Ok(sections);
        }

        [HttpGet("by-department/{departmentId}")]
        public async Task<IActionResult> GetSectionsByDepartment(Guid departmentId)
        {
            var sections = await _sectionService.GetSectionsByDepartment(departmentId);
            return Ok(sections);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSection([FromBody] SectionDto sectionDto)
        {
            try
            {
                var result = await _sectionService.CreatedSection(sectionDto);
                if (result) return Ok();
                return BadRequest("Không thể tạo Section");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSection(Guid id, [FromBody] SectionDto sectionDto)
        {
            try
            {
                var result = await _sectionService.UpdateSection(sectionDto);
                if (result) return Ok();
                return BadRequest("Không thể cập nhật Section");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteSection(Guid id)
        {
            try
            {
                var result = await _sectionService.DeleteSection(id);
                if (result) return Ok();
                return BadRequest("Không thể xóa Section");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
