using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebService.Helpers.Interfaces;
using WebService.Models.Dtos;
using WebService.Models.Dtos.Auth;
using WebService.Providers.Interfaces;

namespace WebService.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(IAuthHelper authHelper, IAuthContextProvider authContextProvider) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        return Ok(await authHelper.SignUp(model));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        return Ok(await authHelper.SignIn(model));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] string refreshToken)
    {
        return Ok(await authHelper.Refresh(refreshToken));
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(User.Identity!.Name + authContextProvider.GetUserEmail());
    }
}