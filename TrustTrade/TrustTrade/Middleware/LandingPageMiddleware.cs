namespace TrustTrade.Middleware
{
    public class LandingPageMiddleware
    {
        private readonly RequestDelegate _next;

        public LandingPageMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
{
    // Only intercept requests to the root path
    if (context.Request.Path == "/" || context.Request.Path == "")
    {
        // Preserve the query string for redirects
        string queryString = context.Request.QueryString.ToString();
        
        // Check if user has visited before using a cookie
        bool hasVisitedBefore = context.Request.Cookies.ContainsKey("HasVisitedBefore");
        bool isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;

        if (!hasVisitedBefore && !isAuthenticated)
        {
            // First visit and not logged in: Show Landing page
            context.Response.Cookies.Append("HasVisitedBefore", "true", new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(3), // Cookie expires after 3 day
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });
            
            context.Response.Redirect("/Home/Landing" + queryString);
            return;
        }
        else if (isAuthenticated)
        {
            // Logged in user: Show Index page
            context.Response.Redirect("/Home/Index" + queryString);
            return;
        }
        else
        {
            // Not first visit, not logged in: Show Index page
            context.Response.Redirect("/Home/Index" + queryString);
            return;
        }
    }

    // For non-root paths, continue normal middleware pipeline
    await _next(context);
}
    }
}
