using TrustTrade.Models;
using TrustTrade.DAL.Abstract;
using TrustTrade.Services.Web.Interfaces;
using TrustTrade.ViewModels;
using TrustTrade.Helpers;

namespace TrustTrade.Services.Web.Implementations;

/// <summary>
/// Service for post-related operations.
/// </summary>
public class PostService : IPostService
{
    private readonly ILogger<PostService> _logger;
    private readonly IPostRepository _postRepository;
    private readonly ITagRepository _tagRepository;
    private const int PAGE_SIZE = 10;
    private const int MAX_PAGES_TO_SHOW = 7;

    public PostService(ILogger<PostService> logger, IPostRepository postRepository, ITagRepository tagRepository)
    {
        _logger = logger;
        _postRepository = postRepository;
        _tagRepository = tagRepository;
    }

    public async Task<List<PostPreviewVM>> GetPostPreviewsAsync(string? categoryFilter, int pageNumber, string sortOrder)
    {
        List<Post> posts = await _postRepository.GetPagedPostsAsync(categoryFilter, pageNumber, PAGE_SIZE, sortOrder);

        return MapPostsToPostPreviewVM(posts);
    }

    public async Task<List<PostPreviewVM>> GetFollowingPostPreviewsAsync(int currentUserId, string? categoryFilter, int pageNumber, string sortOrder)
    {
        List<Post> posts = await _postRepository.GetPagedPostsByUserFollowsAsync(currentUserId, categoryFilter, pageNumber, PAGE_SIZE, sortOrder);

        return MapPostsToPostPreviewVM(posts);
    }

    public async Task<List<PostPreviewVM>> GetUserPostPreviewsAsync(int userId, string? categoryFilter, int pageNumber, string sortOrder)
    {
        List<Post> posts = await _postRepository.GetPagedPostsByUserAsync(userId, categoryFilter, pageNumber, PAGE_SIZE, sortOrder);

        return MapPostsToPostPreviewVM(posts);
    }

    public async Task<PostFiltersPartialVM> BuildPostFiltersAsync(string? categoryFilter, string sortOrder)
    {
        List<string> tagNames = await _tagRepository.GetAllTagNamesAsync();

        return new PostFiltersPartialVM
        {
            SelectedCategory = categoryFilter,
            SortOrder = sortOrder,
            Categories = tagNames
        };
    }

    public async Task<PaginationPartialVM> BuildPaginationAsync(string? categoryFilter, int pageNumber)
    {
        int totalPosts = await _postRepository.GetTotalPostsAsync(categoryFilter);

        return MapToPaginationPartialVM(pageNumber, totalPosts, categoryFilter);
    }

    public async Task<PaginationPartialVM> BuildFollowingPaginationAsync(int currentUserId, string? categoryFilter, int pageNumber)
    {
        int totalPosts = await _postRepository.GetTotalPostsByUserFollowsAsync(currentUserId, categoryFilter);
        
        return MapToPaginationPartialVM(pageNumber, totalPosts, categoryFilter);
    }

    public async Task<PaginationPartialVM> BuildUserPaginationAsync(int userId, string? categoryFilter, int pageNumber)
    {
        int totalPosts = await _postRepository.GetTotalPostsByUserAsync(userId, categoryFilter);

        return MapToPaginationPartialVM(pageNumber, totalPosts, categoryFilter);
    }

    private static List<PostPreviewVM> MapPostsToPostPreviewVM(List<Post> posts)
    {
        return posts.Select(p => new PostPreviewVM
        {
            Id = p.Id,
            UserName = p.User.Username,
            Title = p.Title,
            Excerpt = p.Content != null && p.Content.Length > 100 
                ? $"{p.Content.Substring(0, 100)}..." 
                : p.Content ?? string.Empty,
            TimeAgo = TimeAgoHelper.GetTimeAgo(p.CreatedAt),
            LikeCount = p.Likes.Count,
            CommentCount = p.Comments.Count,
            IsPlaidEnabled = p.User.PlaidEnabled ?? false,
            PortfolioValueAtPosting = p.PortfolioValueAtPosting,
            ProfilePicture = p.User.ProfilePicture
        }).ToList();
    }

    private static PaginationPartialVM MapToPaginationPartialVM(int currentPage, int totalPosts, string? categoryFilter)
    {
        int totalPages = (int)Math.Ceiling((double)totalPosts / PAGE_SIZE);

        return new PaginationPartialVM
        {
            CurrentPage = currentPage,
            TotalPages = totalPages,
            PagesToShow = PaginationHelper.GetPagination(currentPage, totalPages, MAX_PAGES_TO_SHOW),
            CategoryFilter = categoryFilter
        };
    }
}
