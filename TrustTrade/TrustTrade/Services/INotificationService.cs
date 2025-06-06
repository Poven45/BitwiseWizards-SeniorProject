using TrustTrade.Models;

namespace TrustTrade.Services
{
    public interface INotificationService
    {
        Task<List<Notification>> GetUnreadNotificationsAsync(int userId, int count = 10);
        Task<int> GetUnreadCountAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId, int currentUserId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task CreateFollowNotificationAsync(int followerId, int followingUserId);
        Task CreateLikeNotificationAsync(int actorId, int postId, int postOwnerId);
        Task CreateCommentNotificationAsync(int actorId, int postId, int postOwnerId, int commentId);
        Task CreateMentionNotificationAsync(int actorId, int entityId, string entityType, int mentionedUserId);
        Task CreateMessageNotificationAsync(int senderId, int recipientId, int conversationId);
        Task<List<Notification>> GetAllNotificationsAsync(int userId, int page = 1, int pageSize = 20);
        Task<int> GetTotalNotificationsCountAsync(int userId);
        Task<bool> ArchiveNotificationAsync(int notificationId, int currentUserId);
        Task<bool> ArchiveAllNotificationsAsync(int userId);
        Task CreateNotificationAsync(int userId, string type, string message, int? entityId, string entityType, int? actorId);
    }
}