using AquaSolution.Server.Services.MenusService;
using AquaSolution.Shared.CommonDto;
using AquaSolution.Shared.Menus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AquaSolution.Server.Controllers.MenuManagements
{

    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        /// <summary>
        /// Lấy danh sách menu + page theo user (dựa theo role)
        /// </summary>
        [HttpGet("GetMenu/{userId}")]
        public async Task<IActionResult> GetMenuTree(Guid userId)
        {
            var result = await _menuService.GetMenuTreeByUserId(userId);
            return Ok(result);
        }
        [HttpGet("GetAllMenu")]
        public async Task<IActionResult> GetAllMenuTree()
        {
            var result = await _menuService.GetAllMenuTree();
            return Ok(result);
        }

        [HttpGet("get-all-list")]
        public async Task<ActionResult<List<BaseDto>>> GetAllMenuList()
        {
            var result = await _menuService.GetAllMenu();
            return Ok(result);
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateMenu([FromBody] HandleMenuDto handleMenuDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _menuService.CreatedMenu(handleMenuDto);
            if (result)
                return Ok(new { success = true, message = "Menu created successfully." });

            return StatusCode(500, new { success = false, message = "Failed to create menu." });
        }
    }

}
