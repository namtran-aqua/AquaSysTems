using AntDesign;
using AquaSolution.Server.Services.MenusService;
using AquaSolution.Server.Services.PageManagement;
using AquaSolution.Shared.Menus;
using AquaSolution.Shared.Pages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.Pages
{
    [ApiController]
    [Route("api/[controller]")]
    public class PageController : ControllerBase
    {
        private readonly IPageService _pageService;

        public PageController(IPageService pageService)
        {
            _pageService = pageService;
        }

        [HttpGet("GetPageIdByUrl/{url}")]
        public async Task<IActionResult> GetPageByURL(string url)
        {
            var result = await _pageService.GetPageByURL(url);
            return Ok(result);
        }
        [HttpGet("GetPageByMenuId/{menuId}")]
        public async Task<IActionResult> GetPageByMenuId(Guid menuId)
        {
            var result = await _pageService.GetPagesByMenu(menuId);
            return Ok(result);
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateMenu([FromBody] HandlePageDto handlePageDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _pageService.CreatedPage(handlePageDto);
            if (result)
                return Ok(new { success = true, message = "Page created successfully." });

            return StatusCode(500, new { success = false, message = "Failed to create Page." });
        }
    }

}
