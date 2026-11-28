using System.Security.Claims;
using Ecosystem.Application.Common;
using Ecosystem.Application.DTOs;
using Ecosystem.WebAPI.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace Ecosystem.UnitTests.WebAPI;

public class RealtimeHubsTests
{
    [Fact]
    public void ClassroomPresenceTracker_EvictsOnlyStaleConnectionsAfter30Seconds()
    {
        // Arrange
        var currentTime = new DateTime(2026, 11, 29, 0, 0, 0, DateTimeKind.Utc);
        var tracker = new ClassroomPresenceTracker(() => currentTime);

        tracker.JoinRoom("conn-1", "NET8-ROOM", "u1", "alice", AuthConstants.Roles.Audience);
        tracker.JoinRoom("conn-2", "NET8-ROOM", "u2", "bob", AuthConstants.Roles.Presenter);

        // Advance 20s and refresh only conn-2 heartbeat
        currentTime = currentTime.AddSeconds(20);
        Assert.True(tracker.UpdateHeartbeat("conn-2"));

        // Advance another 15s (total 35s for conn-1, 15s since heartbeat for conn-2)
        currentTime = currentTime.AddSeconds(15);

        // Act
        var evicted = tracker.EvictStaleConnections(TimeSpan.FromSeconds(30));

        // Assert
        var onlyEvicted = Assert.Single(evicted);
        Assert.Equal("conn-1", onlyEvicted.ConnectionId);
        Assert.Single(tracker.GetRoomParticipants("NET8-ROOM"));
        Assert.NotNull(tracker.GetParticipant("conn-2"));
    }

    [Fact]
    public async Task CollaborationHub_JoinCursorAndFloatingMessage_BroadcastsCorrectEvents()
    {
        // Arrange
        var othersProxyMock = new Mock<IClientProxy>();
        var groupProxyMock = new Mock<IClientProxy>();
        var clientsMock = new Mock<IHubCallerClients>();
        var groupsMock = new Mock<IGroupManager>();
        var contextMock = CreateHubContext("conn-collab-1", "u-1", "presenter_demo", AuthConstants.Roles.Presenter);

        clientsMock.Setup(c => c.OthersInGroup("doc:article-123")).Returns(othersProxyMock.Object);
        clientsMock.Setup(c => c.Group("doc:article-123")).Returns(groupProxyMock.Object);

        var hub = new CollaborationHub
        {
            Clients = clientsMock.Object,
            Groups = groupsMock.Object,
            Context = contextMock.Object
        };

        // Act
        await hub.JoinDocument("article-123");
        await hub.SendCursorPosition(new CursorPositionDto
        {
            ArticleId = "article-123",
            X = 120.5,
            Y = 340.0,
            BlockPercent = 45.2,
            NodeId = "h2-architecture"
        });
        await hub.SendFloatingMessage(new FloatingMessageDto
        {
            ArticleId = "article-123",
            Content = "Đoạn này rất hay!",
            AnchorBlockId = "para-4",
            OffsetY = 12
        });

        // Assert
        groupsMock.Verify(g => g.AddToGroupAsync("conn-collab-1", "doc:article-123", It.IsAny<CancellationToken>()), Times.Once);
        othersProxyMock.Verify(p => p.SendCoreAsync(
            "ReceiveCursorPosition",
            It.Is<object?[]>(args => args.Length == 1 && args[0] is UserCursorDto),
            It.IsAny<CancellationToken>()), Times.Once);
        groupProxyMock.Verify(p => p.SendCoreAsync(
            "ReceiveFloatingMessage",
            It.Is<object?[]>(args => args.Length == 1 && args[0] is FloatingMessageDto),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CollaborationHub_WhenFloatingMessageEmpty_ThrowsHubException()
    {
        var hub = new CollaborationHub
        {
            Context = CreateHubContext("conn-1", "u-1", "user_demo", AuthConstants.Roles.Audience).Object
        };

        await Assert.ThrowsAsync<HubException>(() => hub.SendFloatingMessage(new FloatingMessageDto
        {
            ArticleId = "article-1",
            Content = "   "
        }));
    }

    [Fact]
    public async Task ClassroomHub_AudienceCannotBroadcastSlideChange_WhilePresenterSucceedsAndAudienceCanHandRaise()
    {
        // Arrange
        var tracker = new ClassroomPresenceTracker();
        var groupProxyMock = new Mock<IClientProxy>();
        var clientsMock = new Mock<IHubCallerClients>();
        var groupsMock = new Mock<IGroupManager>();
        clientsMock.Setup(c => c.Group("room:CSHARP-101")).Returns(groupProxyMock.Object);

        // 1. Audience joins and tries to change slide -> HubException
        var audienceHub = new ClassroomHub(tracker)
        {
            Clients = clientsMock.Object,
            Groups = groupsMock.Object,
            Context = CreateHubContext("conn-aud", "u-aud", "user_demo", AuthConstants.Roles.Audience).Object
        };

        await audienceHub.JoinClassroom("CSHARP-101");
        await Assert.ThrowsAsync<HubException>(() => audienceHub.BroadcastSlideChange(new SlideStateDto
        {
            RoomCode = "CSHARP-101",
            SlideIndex = 5
        }));

        // Audience CAN raise hand -> ReceiveHandRaise
        await audienceHub.HandRaise("u-aud");
        groupProxyMock.Verify(p => p.SendCoreAsync(
            "ReceiveHandRaise",
            It.Is<object?[]>(args => args.Length == 1 && args[0] is HandRaiseNotificationDto),
            It.IsAny<CancellationToken>()), Times.Once);

        // 2. Presenter joins and broadcasts slide change -> ReceiveSlideChange
        var presenterHub = new ClassroomHub(tracker)
        {
            Clients = clientsMock.Object,
            Groups = groupsMock.Object,
            Context = CreateHubContext("conn-pres", "u-pres", "presenter_demo", AuthConstants.Roles.Presenter).Object
        };

        await presenterHub.JoinClassroom("CSHARP-101");
        await presenterHub.BroadcastSlideChange(new SlideStateDto
        {
            RoomCode = "CSHARP-101",
            SlideIndex = 12,
            SectionAnchor = "#ef-core-linq"
        });

        groupProxyMock.Verify(p => p.SendCoreAsync(
            "ReceiveSlideChange",
            It.Is<object?[]>(args => args.Length == 1 && ((SlideStateDto)args[0]!).SlideIndex == 12),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Mock<HubCallerContext> CreateHubContext(string connectionId, string userId, string username, string role)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            },
            "Bearer"));

        var contextMock = new Mock<HubCallerContext>();
        contextMock.Setup(c => c.ConnectionId).Returns(connectionId);
        contextMock.Setup(c => c.User).Returns(principal);
        return contextMock;
    }
}
