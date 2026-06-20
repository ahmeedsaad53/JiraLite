using JiraLiteAPI.Data.Context;
using JiraLiteAPI.Enum;

namespace JiraLiteAPI.Data.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public ActivityType Action { get; set; }
        public string UserId { get; set; }
        public int TaskId { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } 
        public ApplicationUser User { get; set; }
        public WorkTask Task { get; set; }

    }
}
