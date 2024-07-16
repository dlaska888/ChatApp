namespace WebService.Models.Dtos.Groups;

public class GetGroupDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public List<string> UserIds { get; set; } = new();
}