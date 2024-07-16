using WebService.Models.Entities;

namespace WebService.Models.Dtos.Account;

public class GetAccountDto
{
    public string Id { get; set; } = null!;
    
    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool EmailConfirmed { get; set; }
    
    public bool TwoFactorEnabled { get; set; }

    public IEnumerable<string> Roles { get; set; } = null!;
}