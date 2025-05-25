using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Moq;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using TrustTrade.Services;
using TrustTrade.Services.Web.Implementations;

namespace TestTrustTrade
{
    [TestFixture]
    public class ReportServiceTests
    {
        private Mock<IReportRepository> _reportRepositoryMock;
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<IPostRepository> _postRepositoryMock;
        private Mock<INotificationService> _notificationServiceMock;
        private Mock<IEmailSender> _emailSenderMock;
        private Mock<ILogger<ReportService>> _loggerMock;
        private ReportService _reportService;

        [SetUp]
        public void Setup()
        {
            _reportRepositoryMock = new Mock<IReportRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _postRepositoryMock = new Mock<IPostRepository>();
            _notificationServiceMock = new Mock<INotificationService>();
            _emailSenderMock = new Mock<IEmailSender>();
            _loggerMock = new Mock<ILogger<ReportService>>();

            _reportService = new ReportService(
                _reportRepositoryMock.Object,
                _userRepositoryMock.Object,
                _postRepositoryMock.Object,
                _notificationServiceMock.Object,
                _emailSenderMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public async Task CreateReportAsync_PostReport_WithValidData_CreatesReportSuccessfully()
        {
            // Arrange
            var reporterId = 1;
            var postId = 2;
            var postOwnerId = 3;
            var category = "Spam";
            var description = "This post is spam";

            var post = new Post
            {
                Id = postId,
                UserId = postOwnerId,
                Title = "Test Post",
                Content = "Test Content",
                User = new User { Id = postOwnerId, Username = "PostOwner" }
            };

            var basicReport = new Report
            {
                Id = 1,
                ReporterId = reporterId,
                ReportType = "Post",
                ReportedPostId = postId,
                ReportedUserId = postOwnerId,
                Category = category,
                Description = description,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            var detailedReport = new Report
            {
                Id = 1,
                ReporterId = reporterId,
                ReportType = "Post",
                ReportedPostId = postId,
                ReportedUserId = postOwnerId,
                Category = category,
                Description = description,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                Reporter = new User { Id = reporterId, Username = "Reporter", Email = "reporter@test.com" },
                ReportedPost = post,
                ReportedUser = new User { Id = postOwnerId, Username = "PostOwner" }
            };

            var admins = new List<User>
            {
                new User { Id = 10, Username = "Admin1" },
                new User { Id = 11, Username = "Admin2" }
            };

            _postRepositoryMock.Setup(x => x.FindByIdAsync(postId))
                .ReturnsAsync(post);
            _reportRepositoryMock.Setup(x => x.AddOrUpdateAsync(It.IsAny<Report>()))
                .Callback<Report>(r => r.Id = 1);
            _reportRepositoryMock.Setup(x => x.GetReportWithDetailsAsync(1))
                .ReturnsAsync(detailedReport);
            _userRepositoryMock.Setup(x => x.GetAllAdminsAsync())
                .ReturnsAsync(admins);

            // Act
            var result = await _reportService.CreateReportAsync(reporterId, "Post", postId, null, category, description);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.ReporterId, Is.EqualTo(reporterId));
            Assert.That(result.ReportType, Is.EqualTo("Post"));
            Assert.That(result.ReportedPostId, Is.EqualTo(postId));
            Assert.That(result.ReportedUserId, Is.EqualTo(postOwnerId));
            Assert.That(result.Category, Is.EqualTo(category));
            Assert.That(result.Description, Is.EqualTo(description));
            Assert.That(result.Status, Is.EqualTo("Pending"));

            // Verify that notifications were sent to all admins
            _notificationServiceMock.Verify(x => x.CreateNotificationAsync(
                10, "Report", It.IsAny<string>(), 1, "Report", reporterId), Times.Once);
            _notificationServiceMock.Verify(x => x.CreateNotificationAsync(
                11, "Report", It.IsAny<string>(), 1, "Report", reporterId), Times.Once);

            // Verify email was sent
            _emailSenderMock.Verify(x => x.SendEmailAsync(
                "trusttrade.auth@gmail.com",
                It.Is<string>(subject => subject.Contains("Spam") && subject.Contains("Post")),
                It.Is<string>(body => body.Contains("Test Post") && body.Contains("PostOwner"))
            ), Times.Once);
        }

        [Test]
        public async Task CreateReportAsync_ProfileReport_WithValidData_CreatesReportSuccessfully()
        {
            // Arrange
            var reporterId = 1;
            var reportedUserId = 2;
            var category = "Harassment";
            var description = "This user is harassing others";

            var basicReport = new Report
            {
                Id = 1,
                ReporterId = reporterId,
                ReportType = "Profile",
                ReportedUserId = reportedUserId,
                Category = category,
                Description = description,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            var detailedReport = new Report
            {
                Id = 1,
                ReporterId = reporterId,
                ReportType = "Profile",
                ReportedUserId = reportedUserId,
                Category = category,
                Description = description,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                Reporter = new User { Id = reporterId, Username = "Reporter", Email = "reporter@test.com" },
                ReportedUser = new User { Id = reportedUserId, Username = "ReportedUser" }
            };

            var admins = new List<User>
            {
                new User { Id = 10, Username = "Admin1" }
            };

            _reportRepositoryMock.Setup(x => x.AddOrUpdateAsync(It.IsAny<Report>()))
                .Callback<Report>(r => r.Id = 1);
            _reportRepositoryMock.Setup(x => x.GetReportWithDetailsAsync(1))
                .ReturnsAsync(detailedReport);
            _userRepositoryMock.Setup(x => x.GetAllAdminsAsync())
                .ReturnsAsync(admins);

            // Act
            var result = await _reportService.CreateReportAsync(reporterId, "Profile", null, reportedUserId, category, description);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ReportType, Is.EqualTo("Profile"));
            Assert.That(result.ReportedUserId, Is.EqualTo(reportedUserId));
            Assert.That(result.Category, Is.EqualTo(category));

            // Verify notification was sent
            _notificationServiceMock.Verify(x => x.CreateNotificationAsync(
                10, "Report", It.IsAny<string>(), 1, "Report", reporterId), Times.Once);

            // Verify email was sent
            _emailSenderMock.Verify(x => x.SendEmailAsync(
                "trusttrade.auth@gmail.com",
                It.Is<string>(subject => subject.Contains("Harassment") && subject.Contains("Profile")),
                It.Is<string>(body => body.Contains("ReportedUser"))
            ), Times.Once);
        }

        [Test]
        public async Task CreateReportAsync_PostNotFound_StillCreatesReportWithWarning()
        {
            // Arrange
            var reporterId = 1;
            var postId = 999; // Non-existent post
            var category = "Spam";
            var description = "This post is spam";

            var basicReport = new Report
            {
                Id = 1,
                ReporterId = reporterId,
                ReportType = "Post",
                ReportedPostId = postId,
                Category = category,
                Description = description,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            var detailedReport = new Report
            {
                Id = 1,
                ReporterId = reporterId,
                ReportType = "Post",
                ReportedPostId = postId,
                Category = category,
                Description = description,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                Reporter = new User { Id = reporterId, Username = "Reporter", Email = "reporter@test.com" }
            };

            _postRepositoryMock.Setup(x => x.FindByIdAsync(postId))
                .ReturnsAsync((Post)null);
            _reportRepositoryMock.Setup(x => x.AddOrUpdateAsync(It.IsAny<Report>()))
                .Callback<Report>(r => r.Id = 1);
            _reportRepositoryMock.Setup(x => x.GetReportWithDetailsAsync(1))
                .ReturnsAsync(detailedReport);
            _userRepositoryMock.Setup(x => x.GetAllAdminsAsync())
                .ReturnsAsync(new List<User>());

            // Act
            var result = await _reportService.CreateReportAsync(reporterId, "Post", postId, null, category, description);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ReportedPostId, Is.EqualTo(postId));
            Assert.That(result.ReportedUserId, Is.Null); // Should remain null since post wasn't found

            // Verify warning was logged
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Post with ID {postId} not found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public async Task CreateReportAsync_DetailedReportRetrievalFails_ReturnsBasicReportAndLogsError()
        {
            // Arrange
            var reporterId = 1;
            var postId = 2;
            var category = "Spam";
            var description = "This post is spam";

            var post = new Post { Id = postId, UserId = 3 };

            _postRepositoryMock.Setup(x => x.FindByIdAsync(postId))
                .ReturnsAsync(post);
            _reportRepositoryMock.Setup(x => x.AddOrUpdateAsync(It.IsAny<Report>()))
                .Callback<Report>(r => r.Id = 1);
            _reportRepositoryMock.Setup(x => x.GetReportWithDetailsAsync(1))
                .ReturnsAsync((Report)null); // Simulating failure to retrieve detailed report
            _userRepositoryMock.Setup(x => x.GetAllAdminsAsync())
                .ReturnsAsync(new List<User>());

            // Act
            var result = await _reportService.CreateReportAsync(reporterId, "Post", postId, null, category, description);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Reporter, Is.Null); // Basic report won't have navigation properties loaded

            // Verify error was logged
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to retrieve details for report ID 1")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public async Task HasUserAlreadyReportedAsync_ReturnsTrue_WhenUserHasReported()
        {
            // Arrange
            var userId = 1;
            var reportType = "Post";
            var entityId = 2;

            _reportRepositoryMock.Setup(x => x.HasUserReportedEntityAsync(userId, reportType, entityId))
                .ReturnsAsync(true);

            // Act
            var result = await _reportService.HasUserAlreadyReportedAsync(userId, reportType, entityId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task HasUserAlreadyReportedAsync_ReturnsFalse_WhenUserHasNotReported()
        {
            // Arrange
            var userId = 1;
            var reportType = "Profile";
            var entityId = 2;

            _reportRepositoryMock.Setup(x => x.HasUserReportedEntityAsync(userId, reportType, entityId))
                .ReturnsAsync(false);

            // Act
            var result = await _reportService.HasUserAlreadyReportedAsync(userId, reportType, entityId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task SendReportNotificationsAsync_WithDetailedReport_SendsCorrectNotifications()
        {
            // Arrange
            var report = new Report
            {
                Id = 1,
                ReporterId = 1,
                ReportType = "Post",
                Category = "Spam",
                Reporter = new User { Id = 1, Username = "Reporter" },
                ReportedPost = new Post 
                { 
                    Id = 2, 
                    Title = "Test Post", 
                    User = new User { Id = 3, Username = "PostOwner" } 
                }
            };

            var admins = new List<User>
            {
                new User { Id = 10, Username = "Admin1" },
                new User { Id = 11, Username = "Admin2" }
            };

            _userRepositoryMock.Setup(x => x.GetAllAdminsAsync())
                .ReturnsAsync(admins);

            // Act
            await _reportService.SendReportNotificationsAsync(report);

            // Assert
            _notificationServiceMock.Verify(x => x.CreateNotificationAsync(
                10, "Report", 
                It.Is<string>(msg => msg.Contains("Reporter") && msg.Contains("Test Post") && msg.Contains("PostOwner")), 
                1, "Report", 1), Times.Once);
            _notificationServiceMock.Verify(x => x.CreateNotificationAsync(
                11, "Report", 
                It.Is<string>(msg => msg.Contains("Reporter") && msg.Contains("Test Post") && msg.Contains("PostOwner")), 
                1, "Report", 1), Times.Once);
        }

        [Test]
        public async Task SendReportNotificationsAsync_WithBasicReport_SendsFallbackNotifications()
        {
            // Arrange
            var report = new Report
            {
                Id = 1,
                ReporterId = 1,
                ReportType = "Post",
                ReportedPostId = 2,
                Category = "Spam",
                // Navigation properties are null (basic report)
                Reporter = null,
                ReportedPost = null
            };

            var admins = new List<User>
            {
                new User { Id = 10, Username = "Admin1" }
            };

            _userRepositoryMock.Setup(x => x.GetAllAdminsAsync())
                .ReturnsAsync(admins);

            // Act
            await _reportService.SendReportNotificationsAsync(report);

            // Assert
            _notificationServiceMock.Verify(x => x.CreateNotificationAsync(
                10, "Report", 
                It.Is<string>(msg => msg.Contains("A user") && msg.Contains("post ID 2")), 
                1, "Report", 1), Times.Once);

            // Verify warning was logged for fallback message
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generating fallback report notification message")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public async Task SendReportNotificationsAsync_NoAdminsFound_LogsWarning()
        {
            // Arrange
            var report = new Report
            {
                Id = 1,
                ReporterId = 1,
                ReportType = "Post",
                Category = "Spam"
            };

            _userRepositoryMock.Setup(x => x.GetAllAdminsAsync())
                .ReturnsAsync(new List<User>()); // No admins

            // Act
            await _reportService.SendReportNotificationsAsync(report);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No admin users found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify no notifications were sent
            _notificationServiceMock.Verify(x => x.CreateNotificationAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), 
                Times.Never);
        }

        [Test]
        public async Task SendReportNotificationsAsync_EmailSendingFails_LogsError()
        {
            // Arrange
            var report = new Report
            {
                Id = 1,
                ReporterId = 1,
                ReportType = "Post",
                Category = "Spam",
                Reporter = new User { Id = 1, Username = "Reporter", Email = "reporter@test.com" }
            };

            _userRepositoryMock.Setup(x => x.GetAllAdminsAsync())
                .ReturnsAsync(new List<User>());
            _emailSenderMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Email service unavailable"));

            // Act
            await _reportService.SendReportNotificationsAsync(report);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send report email")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}