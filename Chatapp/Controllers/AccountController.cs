using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebService.Models.Dtos.Account;
using WebService.Models.Entities;
using WebService.Providers.Interfaces;
using WebService.Services.Interfaces;

namespace WebService.Controllers;

[Authorize]
[Route("[controller]")]
public class AccountController(IAuthContextProvider authContextProvider, IAccountService accountService)
    : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<ChatUser>> Me()
    {
        return Ok(await accountService.GetAccountAsync(authContextProvider.GetUserId()));
    }


    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        return Ok(await accountService.ChangePasswordAsync(authContextProvider.GetUserId(), dto));
    }

    [HttpPost("change-username")]
    public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsernameDto dto)
    {
        return Ok(await accountService.ChangeUsernameAsync(authContextProvider.GetUserId(), dto));
    }
}