using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Shared.Models;
using WebService.Providers.Interfaces;
using WebService.Services.Interfaces;

namespace WebService.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ChatController(
    IMessageService messageService,
    IAuthContextProvider contextProvider) : ControllerBase
{
    [HttpGet("messages/{chatTypeEnum}/{receiverId}")]
    public async Task<IActionResult> GetChats(ChatTypeEnum chatTypeEnum, string receiverId,
        [FromQuery] string? earliestMessageId)
    {
        var senderId = contextProvider.GetUserId();
        ObjectId? earliestMessageIdObj = earliestMessageId is not null ? new ObjectId(earliestMessageId) : null;

        var messages = await messageService.GetMessagesByChat(new ObjectId(senderId), new ObjectId(receiverId),
            chatTypeEnum, earliestMessageIdObj);

        return Ok(messages);
    }

    [HttpGet("chats")]
    public async Task<IActionResult> GetChats()
    {
        var userId = contextProvider.GetUserId();
        var chats = await messageService.GetAllChats(new ObjectId(userId));
        return Ok(chats);
    }
}