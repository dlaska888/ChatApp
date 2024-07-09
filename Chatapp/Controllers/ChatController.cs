using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebService.Models;
using WebService.Providers.Interfaces;
using WebService.Services.Interfaces;

namespace WebService.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ChatController(
    IChatService chatService,
    IAuthContextProvider contextProvider) : ControllerBase
{
    [HttpGet("messages/{chatTypeEnum}/{receiverId}")]
    public async Task<IActionResult> GetChats(ChatTypeEnum chatTypeEnum, string receiverId,
        [FromQuery] string? earliestMessageId)
    {
        var senderId = contextProvider.GetUserId()!;

        var messages = await chatService.GetMessagesByChat(senderId, receiverId,
            chatTypeEnum, earliestMessageId);

        return Ok(messages);
    }

    [HttpGet("chats")]
    public async Task<IActionResult> GetAllChats()
    {
        var userId = contextProvider.GetUserId()!;
        var chats = await chatService.GetAllChats(userId);
        return Ok(chats);
    }
}