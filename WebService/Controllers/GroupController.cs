using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using WebService.Models.Entities;
using WebService.Services.Interfaces;

namespace WebService.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class GroupController(IGroupService groupService) : ControllerBase
{
    // Create a new group
    [HttpPost("create")]
    public async Task<ActionResult<Group>> CreateGroup([FromBody] Group group)
    {
        var createdGroup = await groupService.CreateGroupAsync(group);
        return CreatedAtAction(nameof(GetGroupById), new { groupId = createdGroup.Id.ToString() }, createdGroup);
    }

    // Read/Get group by ID
    [HttpGet("{groupId}")]
    public async Task<ActionResult<Group>> GetGroupById(string groupId)
    {
        var group = await groupService.GetGroupByIdAsync(new ObjectId(groupId));
        if (group == null)
        {
            return NotFound();
        }
        return Ok(group);
    }

    // Update group
    [HttpPut("{groupId}")]
    public async Task<IActionResult> UpdateGroup(string groupId, [FromBody] Group updatedGroup)
    {
        var result = await groupService.UpdateGroupAsync(new ObjectId(groupId), updatedGroup);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    // Delete group
    [HttpDelete("{groupId}")]
    public async Task<IActionResult> DeleteGroup(string groupId)
    {
        var result = await groupService.DeleteGroupAsync(new ObjectId(groupId));
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    // Join a group
    [HttpPost("join_group")]
    public async Task<IActionResult> JoinGroup(string userId, string groupId)
    {
        var result = await groupService.AddUserToGroupAsync(new ObjectId(userId), new ObjectId(groupId));
        if (!result)
        {
            return NotFound();
        }
        return Ok();
    }

    // Leave a group
    [HttpPost("leave_group")]
    public async Task<IActionResult> LeaveGroup(string userId, string groupId)
    {
        var result = await groupService.RemoveUserFromGroupAsync(new ObjectId(userId), new ObjectId(groupId));
        if (!result)
        {
            return NotFound();
        }
        return Ok();
    }
}