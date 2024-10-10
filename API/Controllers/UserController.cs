using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync()
        {
            return Ok();
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(string fullname, string email, int id)
        {
           return _userService.UpdateAsync(fullname, email, id);
        }
    }
}