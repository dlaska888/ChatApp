using WebService.Models.Dtos;

namespace WebService.Helpers.Interfaces;

public interface IAuthHelper
{
    Task<TokenDto> SignIn(LoginDto dto);
    Task<bool> SignUp(RegisterDto dto);
    Task<TokenDto> Refresh(string refreshToken);
}