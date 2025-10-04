
using AuthApi.Entities;
using AuthApi.Models;
using AuthApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
     

    {
        private readonly IAuthService authService;
        public AuthController(IAuthService _authService)
        {
            authService = _authService;
        }
        private static User  user = new ();
        [HttpPost("register")]
        public async  Task<ActionResult<User>>  Register(UserDto request)
        {
           var user = authService.RegisterAsync(request);
            if (user == null)
            {
                return BadRequest("Username already exists");
            }
            return Ok(user);
        }
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
          var token = await authService.LoginAsync(request);
            if(token == null)
            {
                return BadRequest("Invalid username or password");
            }
            return Ok(token);
        }

        [HttpGet]
        [Authorize]
        public ActionResult AuthenticatedEndpointOnly()
        {

            return Ok("You are Authenticated");
        }
        [HttpGet("admin-only")]
        [Authorize(Roles = "Admin")]
        public ActionResult AdminEndPoint()
        {

            return Ok("You are Authenticated");
        }



    }
}
