using WebService.Enums;

namespace WebService.Models.Dtos.Messages;

public class GetChatDto
{
    public string Name { get; set; } = null!;
    public string ReceiverId { get; set; } = null!;
    public Enums.ChatTypeEnum ChatTypeEnum { get; set; }
}