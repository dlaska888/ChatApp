namespace WebService.Models;

public class JwtOptions
{
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string Key { get; set; } = null!;
    public int ExpirationTime { get; set; } = 30;
    public int RefreshExpirationTime { get; set; } = 10080;
}