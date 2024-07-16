using Microsoft.AspNetCore.Identity;
using SendGrid.Helpers.Errors.Model;
using WebService.Models.Dtos.Account;
using WebService.Models.Entities;
using WebService.Services.Interfaces;

namespace WebService.Services;

public class AccountService(UserManager<ChatUser> userManager) : IAccountService
{
    public async Task<GetAccountDto> GetAccountAsync(string userId)
    {
        var user = await GetUserById(userId);
        return new GetAccountDto
        {
            Id = user.Id.ToString(),
            UserName = user.UserName!,
            Email = user.Email!,
            CreatedAt = user.CreatedAt,
            EmailConfirmed = user.EmailConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            Roles = await userManager.GetRolesAsync(user)
        };
    }

    public async Task<bool> ChangeUsernameAsync(string userId, ChangeUsernameDto dto)
    {
        var user = await GetUserById(userId);

        if (!await userManager.CheckPasswordAsync(user, dto.Password))
            throw new UnauthorizedException("Invalid login credentials");

        if (user.UserName == dto.NewUsername)
            throw new BadRequestException("Username must be different from the current one");

        user.UserName = dto.NewUsername;

        return CheckResult(await userManager.UpdateAsync(user), "Failed to update username");
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        var user = await GetUserById(userId);

        if (await userManager.CheckPasswordAsync(user, dto.NewPassword))
            throw new BadRequestException("New password must be different from the old one");

        return CheckResult(await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword),
            "Failed to update password");
    }

    #region Private methods

    private bool CheckResult(IdentityResult result, string messageOnFail)
    {
        if (result.Errors.Any())
            throw new BadRequestException(result.Errors.First().Description);

        if (!result.Succeeded)
            throw new BadRequestException(messageOnFail);

        return result.Succeeded;
    }

    private async Task<ChatUser> GetUserById(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null) throw new NotFoundException("No user found");

        return user;
    }

    #endregion
}