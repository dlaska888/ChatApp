using Shared.Models;

namespace WebService.Models.Dtos;

public class GetMessageDto
{
    public string Id { get; set; } = null!;
    public string SenderId { get; set; } = null!;
    public string ReceiverId { get; set; } = null!;
    
    public ChatTypeEnum ChatTypeEnum { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}