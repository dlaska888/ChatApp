namespace WebService.Models.Hubs;

public class HubUser
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public HashSet<string> ConnectionIds { get; set; } = null!;
}