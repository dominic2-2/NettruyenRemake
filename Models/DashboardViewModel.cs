using Microsoft.AspNetCore.Mvc;

namespace NettruyenRemake.Models
{
    public class DashboardViewModel : Controller
    {
        // System Overview
        public List<UserRegistrationData> UserRegistrations { get; set; }
        public List<RoleDistribution> RoleDistributions { get; set; }

        // Comic Insights
        public List<ComicStat> TopViewedComics { get; set; }
        public List<ComicStat> TopFollowedComics { get; set; }
        public List<StatusDistribution> ComicStatusDistribution { get; set; }
        public List<CategoryDistribution> CategoryDistribution { get; set; }

        // User Interaction
        public List<InteractionData> RatingActivity { get; set; }
        public List<InteractionData> CommentActivity { get; set; }
        public List<MostCommentedComic> MostCommentedComics { get; set; }

        // Reading Behavior
        public List<ReadingData> ReadingActivity { get; set; }
        public List<PeakHourData> ReadingPeakHours { get; set; }
    }

    public class UserRegistrationData
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class RoleDistribution
    {
        public string RoleName { get; set; }
        public int Count { get; set; }
    }

    public class StatusDistribution
    {
        public string StatusName { get; set; }
        public int Count { get; set; }
    }

    public class CategoryDistribution
    {
        public string CategoryName { get; set; }
        public int Count { get; set; }
    }

    public class InteractionData
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class ReadingData
    {
        public DateTime Date { get; set; }
        public int ReadCount { get; set; }
    }

    public class PeakHourData
    {
        public int Hour { get; set; }
        public int ReadCount { get; set; }
    }
    public class MostCommentedComic
    {
        public string Title { get; set; }
        public int TotalComments { get; set; }
    }
}

