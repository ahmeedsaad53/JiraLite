namespace JiraLiteAPI.DTO
{
    public class AdminDashboardDTO
    {


        public int TotalProjects { get; set; }
        public int TotalTasks { get; set; }
        public int TotalUsers { get; set; }
        public int PendingRequests { get; set; }

        public TasksStatusSummary TasksStatus { get; set; }

        public List<ActivityDTO> RecentActivity { get; set; }

    }
}
