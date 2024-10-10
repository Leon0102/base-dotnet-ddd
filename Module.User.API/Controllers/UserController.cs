using MediatR;
using Microsoft.AspNetCore.Mvc;
using Module.User.Domain.Services;

namespace User.API.Controllers
{
    [ApiController]
    [Route("/api/users/[controller]")]
    internal class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok("test");
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync()
        {
            return Ok("test");
        }
    }
}