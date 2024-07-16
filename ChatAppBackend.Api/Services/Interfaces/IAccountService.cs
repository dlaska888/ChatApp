using Microsoft.AspNetCore.Identity;
using WebService.Models.Dtos.Account;
using WebService.Models.Entities;

namespace WebService.Services.Interfaces
{
    public interface IAccountService
    {
        Task<GetAccountDto> GetAccountAsync(string userId);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto model);
        Task<bool> ChangeUsernameAsync(string userId, ChangeUsernameDto dto);
    }
}