using Moq;
using MongoDB.Bson;
using WebService.Enums;
using WebService.Exceptions;
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
        private readonly Mock<IMongoRepository<PrivateMessage>> _mockPrivateMessageRepo;
        private readonly Mock<IMongoRepository<GroupMessage>> _mockGroupMessageRepo;
        private readonly Mock<IGroupService> _mockGroupService;
        private readonly ChatService _chatService;

        public ChatServiceTests()
        {
            _mockUserRepo = new Mock<IMongoRepository<ChatUser>>();
            _mockPrivateMessageRepo = new Mock<IMongoRepository<PrivateMessage>>();
            _mockGroupMessageRepo = new Mock<IMongoRepository<GroupMessage>>();
            _mockGroupService = new Mock<IGroupService>();
            _chatService = new ChatService(_mockUserRepo.Object, _mockPrivateMessageRepo.Object,
                _mockGroupMessageRepo.Object, _mockGroupService.Object);
        }

        [Fact]
        public async Task CreatePrivateMessageAsync_ShouldInsertPrivateMessage()
        {
            // Arrange
            var dto = new CreateMessageDto
            {
                SenderId = ObjectId.GenerateNewId().ToString(),
                ReceiverId = ObjectId.GenerateNewId().ToString(),
                Content = "Test private message"
            };

            // Act
            await _chatService.CreatePrivateMessageAsync(dto);

            // Assert
            _mockPrivateMessageRepo.Verify(repo => repo.InsertOneAsync(It.Is<PrivateMessage>(m =>
                m.SenderId == new ObjectId(dto.SenderId) &&
                m.ReceiverId == new ObjectId(dto.ReceiverId) &&
                m.Content == dto.Content
            )), Times.Once);
        }

        [Fact]
        public async Task CreateGroupMessageAsync_ShouldInsertGroupMessage()
        {
            // Arrange
            var dto = new CreateMessageDto
            {
                SenderId = ObjectId.GenerateNewId().ToString(),
                ReceiverId = ObjectId.GenerateNewId().ToString(),
                Content = "Test group message"
            };

            // Act
            await _chatService.CreateGroupMessageAsync(dto);

            // Assert
            _mockGroupMessageRepo.Verify(repo => repo.InsertOneAsync(It.Is<GroupMessage>(m =>
                m.SenderId == new ObjectId(dto.SenderId) &&
                m.GroupId == new ObjectId(dto.ReceiverId) &&
                m.Content == dto.Content
            )), Times.Once);
        }

        [Fact]
        public async Task GetAllChatsAsync_ShouldReturnAllChats()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();

            var privateMessages = new List<PrivateMessage>
            {
                new PrivateMessage
                {
                    SenderId = new ObjectId(userId),
                    ReceiverId = ObjectId.GenerateNewId(),
                    Content = "Hello"
                },
                new PrivateMessage
                {
                    SenderId = ObjectId.GenerateNewId(),
                    ReceiverId = new ObjectId(userId),
                    Content = "Hi"
                }
            }.AsQueryable();

            var users = new List<ChatUser>
            {
                new ChatUser
                {
                    Id = privateMessages.First().ReceiverId,
                    UserName = "User1"
                },
                new ChatUser
                {
                    Id = privateMessages.Last().SenderId,
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

            _mockPrivateMessageRepo.Setup(repo => repo.AsQueryable()).Returns(privateMessages);
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
        public async Task GetPrivateMessagesAsync_ShouldReturnPrivateMessages()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var receiverId = ObjectId.GenerateNewId().ToString();

            var privateMessages = new List<PrivateMessage>
            {
                new PrivateMessage
                {
                    SenderId = new ObjectId(userId),
                    ReceiverId = new ObjectId(receiverId),
                    Content = "Hello"
                },
                new PrivateMessage
                {
                    SenderId = new ObjectId(receiverId),
                    ReceiverId = new ObjectId(userId),
                    Content = "Hi"
                }
            }.AsQueryable();

            _mockPrivateMessageRepo.Setup(repo => repo.AsQueryable()).Returns(privateMessages);

            // Act
            var result = await _chatService.GetPrivateMessagesAsync(userId, receiverId, null);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetGroupMessagesAsync_ShouldReturnGroupMessages()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var groupId = ObjectId.GenerateNewId().ToString();

            var groupMessages = new List<GroupMessage>
            {
                new GroupMessage
                {
                    SenderId = new ObjectId(userId),
                    GroupId = new ObjectId(groupId),
                    Content = "Hello group"
                }
            }.AsQueryable();

            _mockGroupMessageRepo.Setup(repo => repo.AsQueryable()).Returns(groupMessages);
            _mockGroupService.Setup(service => service.UserHasAccessToGroup(userId, groupId)).ReturnsAsync(true);

            // Act
            var result = await _chatService.GetGroupMessagesAsync(userId, groupId, null);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetGroupMessagesAsync_ShouldThrowUnauthorizedExceptionIfUserNotInGroup()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId().ToString();
            var groupId = ObjectId.GenerateNewId().ToString();

            _mockGroupService.Setup(service => service.UserHasAccessToGroup(userId, groupId)).ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await _chatService.GetGroupMessagesAsync(userId, groupId, null));
        }
    }
}