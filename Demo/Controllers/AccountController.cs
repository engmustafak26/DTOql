using Demo.Domain;
using Demo.DTO.Requests;
using DTOql.Continuations;
using DTOql.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {

        private readonly ILogger<AccountController> _logger;
        private readonly IService<User> _userService;

        public AccountController(ILogger<AccountController> logger, IService<User> userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> GetUsersAsync(UserSearchDto dto)
        {
            return Ok(await _userService.GetAsync(typeof(UserListDto),dto));
        }

     

    }
}
