namespace WebService.Models.Dtos.Messages;

public class CreateMessageDto
{
    public string SenderId { get; set; } = null!;
    public string ReceiverId { get; set; } = null!;
    public string Content { get; set; } = null!;
}