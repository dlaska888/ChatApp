namespace WebService.Models.Dtos.Messages;

public class GetChatDto
{
    public string Name { get; set; } = null!;
    public string ReceiverId { get; set; } = null!;
    public ChatTypeEnum ChatTypeEnum { get; set; }
}