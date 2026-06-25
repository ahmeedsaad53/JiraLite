namespace JiraLiteAPI.DTO
{
  
        public class UserDashboardDTO
        {
            public int MyTasks { get; set; }
            public int MyPendingRequests { get; set; }
            public int MyComments { get; set; }

            public TasksStatusSummary MyTasksStatus { get; set; }

            public List<ActivityDTO> RecentActivity { get; set; }
        }

    
}
