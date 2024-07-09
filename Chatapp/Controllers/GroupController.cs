using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebService.Models.Dtos.Groups;
using WebService.Models.Entities;
using WebService.Providers.Interfaces;
using WebService.Services.Interfaces;

namespace WebService.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class GroupController(IGroupService groupService, IAuthContextProvider contextProvider) : ControllerBase
{
    // Create a new group
    [HttpPost("create")]
    public async Task<ActionResult<Group>> CreateGroup([FromBody] CreateGroupDto group)
    {
        var userId = contextProvider.GetUserId();
        var createdGroup = await groupService.CreateGroupAsync(userId, group);
        return CreatedAtAction(nameof(GetGroupById), new { groupId = createdGroup.Id.ToString() }, createdGroup);
    }

    // Read/Get group by ID
    [HttpGet("{groupId}")]
    public async Task<ActionResult<Group>> GetGroupById(string groupId)
    {
        var userId = contextProvider.GetUserId();
        var group = await groupService.GetGroupByIdAsync(userId, groupId);
        return Ok(group);
    }

    // Update group
    [HttpPut("{groupId}")]
    public async Task<IActionResult> UpdateGroup(string groupId, [FromBody] UpdateGroupDto updatedGroup)
    {
        await groupService.UpdateGroupAsync(groupId, updatedGroup);
        return NoContent();
    }

    // Delete group
    [HttpDelete("{groupId}")]
    public async Task<IActionResult> DeleteGroup(string groupId)
    {
        var userId = contextProvider.GetUserId();
        await groupService.DeleteGroupAsync(userId, groupId);
        return NoContent();
    }

    // Join a group
    [HttpPost("join")]
    public async Task<IActionResult> JoinGroup(string userId, string groupId)
    {
        var result = await groupService.AddUserToGroupAsync(userId, groupId);
        if (!result)
        {
            return NotFound();
        }

        return Ok();
    }

    // Leave a group
    [HttpPost("leave")]
    public async Task<IActionResult> LeaveGroup(string userId, string groupId)
    {
        var result = await groupService.RemoveUserFromGroupAsync(userId, groupId);
        if (!result)
        {
            return NotFound();
        }

        return Ok();
    }
}