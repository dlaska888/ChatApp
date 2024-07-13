using MongoDB.Bson;
using Moq;
using WebService.Exceptions;
using WebService.Models;
using WebService.Models.Dtos;
using WebService.Models.Dtos.Groups;
using WebService.Models.Dtos.Messages;
using WebService.Models.Entities;
using WebService.Repositories.Interfaces;
using WebService.Services;
using WebService.Services.Interfaces;

namespace ChatApp.Tests.Tests
{
    public class ChatServiceTests
    {
        private readonly Mock<IMongoRepository<ChatUser>> _mockUserRepo;
        private readonly Mock<IMongoRepository<Message>> _mockMessageRepo;
        private readonly Mock<IGroupService> _mockGroupService;
        private readonly ChatService _chatService;

        public ChatServiceTests()
        {
            _mockUserRepo = new Mock<IMongoRepository<ChatUser>>();
            _mockMessageRepo = new Mock<IMongoRepository<Message>>();
            _mockGroupService = new Mock<IGroupService>();
            _chatService = new ChatService(_mockUserRepo.Object, _mockMessageRepo.Object, _mockGroupService.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldCallInsertOneAsync()
        {
            // Arrange
            var dto = new CreateMessageDto
            {
                SenderId = ObjectId.GenerateNewId().ToString(),
                ReceiverId = ObjectId.GenerateNewId().ToString(),
                Content = "Test message"
            };

            // Act
            await _chatService.CreateAsync(dto);

            // Assert
            _mockMessageRepo.Verify(repo => repo.InsertOneAsync(It.Is<Message>(m =>
                m.SenderId == new ObjectId(dto.SenderId) &&
                m.ReceiverId == new ObjectId(dto.ReceiverId) &&
                m.Content == dto.Content
            )), Times.Once);
        }

        [Fact]
        public async Task GetAllChats_ShouldReturnAllChats()
        {
            // Arrange
            var userObjectId = ObjectId.GenerateNewId();
            var userId = userObjectId.ToString();

            var messages = new List<Message>
            {
                new()
                {
                    SenderId = userObjectId,
                    ReceiverId = ObjectId.GenerateNewId(),
                    Content = "Hello"
                },
                new()
                {
                    SenderId = ObjectId.GenerateNewId(),
                    ReceiverId = userObjectId,
                    Content = "Hi"
                }
            }.AsQueryable();

            var users = new List<ChatUser>
            {
                new()
                {
                    Id = messages.First().ReceiverId,
                    UserName = "User1"
                },
                new()
                {
                    Id = messages.Last().SenderId,
                    UserName = "User2"
                }
            }.AsQueryable();

            var groups = new List<GetGroupDto>
            {
                new GetGroupDto
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Name = "Group1",
                    UserIds = new List<string> { userId }
                }
            };

            _mockMessageRepo.Setup(repo => repo.AsQueryable()).Returns(messages);
            _mockUserRepo.Setup(repo => repo.AsQueryable()).Returns(users);
            _mockGroupService.Setup(s => s.GetAllGroupsAsync(It.IsAny<string>())).ReturnsAsync(groups);

            // Act
            var chats = await _chatService.GetAllChatsAsync(userId);

            // Assert
            Assert.Equal(3, chats.Count());
            Assert.Contains(chats, chat => chat.ChatTypeEnum == ChatTypeEnum.Private && chat.Name == "User1");
            Assert.Contains(chats, chat => chat.ChatTypeEnum == ChatTypeEnum.Private && chat.Name == "User2");
            Assert.Contains(chats, chat => chat.ChatTypeEnum == ChatTypeEnum.Group && chat.Name == "Group1");
        }

        [Fact]
        public async Task GetMessagesByChat_ShouldReturnMessagesForPrivateChat()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var receiverId = ObjectId.GenerateNewId().ToString();
            var userObjectId = new ObjectId(userId);
            var receiverObjectId = new ObjectId(receiverId);

            var messages = new List<Message>
            {
                new Message
                {
                    SenderId = userObjectId,
                    ReceiverId = receiverObjectId,
                    Content = "Hello",
                    Id = ObjectId.GenerateNewId()
                },
                new Message
                {
                    SenderId = receiverObjectId,
                    ReceiverId = userObjectId,
                    Content = "Hi",
                    Id = ObjectId.GenerateNewId()
                }
            }.AsQueryable();

            _mockMessageRepo.Setup(repo => repo.AsQueryable()).Returns(messages);

            // Act
            var result = await _chatService.GetMessagesByChatAsync(userId, receiverId, ChatTypeEnum.Private, null);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetMessagesByChat_ShouldReturnMessagesForGroupChat()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var groupId = ObjectId.GenerateNewId().ToString();
            var userObjectId = new ObjectId(userId);
            var groupObjectId = new ObjectId(groupId);

            var messages = new List<Message>
            {
                new Message
                {
                    SenderId = userObjectId,
                    ReceiverId = groupObjectId,
                    Content = "Hello group",
                    Id = ObjectId.GenerateNewId()
                }
            }.AsQueryable();

            var group = new Group
            {
                Id = groupObjectId,
                UserIds = new List<ObjectId> { userObjectId }
            };

            _mockMessageRepo.Setup(repo => repo.AsQueryable()).Returns(messages);
            _mockGroupService.Setup(service => service.UserHasAccessToGroup(userId, groupId)).ReturnsAsync(true);

            // Act
            var result = await _chatService.GetMessagesByChatAsync(userId, groupId, ChatTypeEnum.Group, null);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetMessagesByChat_ShouldThrowUnauthorizedAccessExceptionIfUserNotInGroup()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var groupId = ObjectId.GenerateNewId().ToString();

            _mockGroupService.Setup(service => service.UserHasAccessToGroup(userId, groupId)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await _chatService.GetMessagesByChatAsync(userId, groupId, ChatTypeEnum.Group, null));
        }
    }
}