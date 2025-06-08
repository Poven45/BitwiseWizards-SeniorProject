namespace TrustTrade.Helpers;

public static class TimeAgoHelper
{
    public static string GetTimeAgo(DateTime pastDate, DateTime? currentDate = null)
    {
        DateTime now = currentDate ?? DateTime.UtcNow.ToLocalTime(); // If currentDate is not provided, use Datetime.UtcNow

        TimeSpan timeSince = now - pastDate;

        if (timeSince.TotalMinutes < 1)
        {
            return "Just now";
        }
        if (timeSince.TotalMinutes < 60)
        {
            int minutes = timeSince.Minutes;
            return minutes == 1 ? "1 minute ago" : $"{minutes} minutes ago";
        }
        if (timeSince.TotalHours < 24)
        {
            int hours = timeSince.Hours;
            return hours == 1 ? "1 hour ago" : $"{hours} hours ago";
        }
        if (timeSince.TotalDays < 30)
        {
            int days = timeSince.Days;
            return days == 1 ? "1 day ago" : $"{days} days ago";
        }
        if (timeSince.TotalDays < 365)
        {
            int months = timeSince.Days / 30;
            return months == 1 ? "1 month ago" : $"{months} months ago";
        }
        int years = timeSince.Days / 365;
        return years == 1 ? "1 year ago" : $"{years} years ago";
    }
}