namespace WebService.Models.Dtos.Account;

public class ChangeUsernameDto
{
    public string NewUsername { get; set; } = null!;
    public string Password { get; set; } = null!;
}