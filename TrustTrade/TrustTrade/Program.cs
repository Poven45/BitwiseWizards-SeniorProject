using Going.Plaid;
using Microsoft.EntityFrameworkCore;
using TrustTrade.Models;
using TrustTrade.DAL.Abstract;
using TrustTrade.DAL.Concrete;
using TrustTrade.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using TrustTrade.Hubs;
using TrustTrade.Middleware;
using TrustTrade.Services;
using TrustTrade.Services.Background;
using TrustTrade.Services.Web.Interfaces;
using TrustTrade.Services.Web.Implementations;
using System.ComponentModel;


var builder = WebApplication.CreateBuilder(args);

// Database Contexts
// Old azure database
/*builder.Services.AddDbContext<TrustTradeDbContext>(options => options
    .UseLazyLoadingProxies()
    .UseSqlServer(builder.Configuration.GetConnectionString("TrustTradeConnection"), sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure();
        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    }));*/

// New postgres database
builder.Services.AddDbContext<TrustTradeDbContext>(options => options
    .UseLazyLoadingProxies()
    .UseNpgsql(builder.Configuration.GetConnectionString("TrustTradePG"), npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure();
        npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    }));

// Repositories
builder.Services.AddScoped<DbContext, TrustTradeDbContext>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IHoldingsRepository, HoldingsRepository>();
builder.Services.AddScoped<ISearchUserRepository, SearchUserRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IVerificationHistoryRepository, VerificationHistoryRepository>();
builder.Services.AddScoped<IPerformanceScoreRepository, PerformanceScoreRepository>();
builder.Services.AddScoped<IMarketRepository, MarketRepository>();
builder.Services.AddScoped<IFinancialNewsRepository, FinancialNewsRepository>();
builder.Services.AddScoped<IUserBlockRepository, UserBlockRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<ICommentLikeRepository, CommentLikeRepository>();
builder.Services.AddScoped<ISavedPostRepository, SavedPostRepository>();
builder.Services.AddScoped<IPhotoRepository, PhotoRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<ISiteSettingsRepository, SiteSettingsRepository>();

// Old azure database
/*var identityConnectionString = builder.Configuration.GetConnectionString("IdentityConnection") 
    ?? throw new InvalidOperationException("Connection string 'IdentityConnection' not found.");
builder.Services.AddDbContext<AuthenticationDbContext>(options =>
    options.UseSqlServer(identityConnectionString));*/

// New postgres database
var identityConnectionString = builder.Configuration.GetConnectionString("IdentityConnectionPG") 
                               ?? throw new InvalidOperationException("Connection string 'IdentityConnection' not found.");
builder.Services.AddDbContext<AuthenticationDbContext>(options =>
    options.UseNpgsql(identityConnectionString));

// Development Tools
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity and Authentication Configuration
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false; // Set to true in production
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AuthenticationDbContext>();

// Cookie Configuration
builder.Services.ConfigureApplicationCookie(options => {
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.Cookie.Name = "TrustTrade.Auth";
});

// Add Plaid services
builder.Services.AddPlaid(builder.Configuration.GetSection("Plaid"));


// MVC Configuration
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Email Sender Configuration
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);

// Add services to the container.
builder.Services.AddScoped<IFinancialNewsService, AlphaVantageNewsService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<ISaveService, SaveService>();
builder.Services.AddScoped<IReportService, ReportService>();

// Add services for performance scoring
builder.Services.AddScoped<IPerformanceScoreRepository, PerformanceScoreRepository>();

// Add services for verification history
builder.Services.AddScoped<IVerificationHistoryRepository, VerificationHistoryRepository>();

builder.Services.AddScoped<IMarketRepository, MarketRepository>();

// Add HttpClient factory
builder.Services.AddHttpClient();

// Register DAL services for fin news
builder.Services.AddScoped<IFinancialNewsRepository, FinancialNewsRepository>();

// Register notifications repositories
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Register admin repositories
builder.Services.AddScoped<IAdminRepository, AdminRepository>();

// Register notifications services
builder.Services.AddScoped<INotificationService, NotificationService>();

// Add support for real-time updates
builder.Services.AddSignalR();

// Register background service - only in production environments
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<FinancialNewsBackgroundService>();
}

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Development vs Production Configuration
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithRedirects("/Home/Error/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Authentication Middleware - ORDER IS CRITICAL
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<SuspensionMiddleware>();
app.UseMiddleware<LandingPageMiddleware>();

app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<ChatHub>("/chatHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Keep only the default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "following",
    pattern: "Home/Following",
    defaults: new { controller = "Home", action = "Following", isFollowing = true});

app.MapRazorPages();

app.Run();