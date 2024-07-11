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
    [HttpGet("private/messages/{receiverId}")]
    public async Task<IActionResult> GetPrivateMessages(string receiverId, [FromQuery] string? earliestMessageId)
    {
        var senderId = contextProvider.GetUserId();
        var messages = await chatService.GetPrivateMessagesAsync(
            senderId,
            receiverId,
            earliestMessageId);
        return Ok(messages);
    }

    [HttpGet("group/messages/{groupId}")]
    public async Task<IActionResult> GetGroupMessages(string groupId, [FromQuery] string? earliestMessageId)
    {
        var senderId = contextProvider.GetUserId();
        var messages = await chatService.GetGroupMessagesAsync(
            senderId,
            groupId,
            earliestMessageId);
        return Ok(messages);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllChats()
    {
        var userId = contextProvider.GetUserId();
        var chats = await chatService.GetAllChatsAsync(userId);
        return Ok(chats);
    }
}