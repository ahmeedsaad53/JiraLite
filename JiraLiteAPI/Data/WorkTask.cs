using JiraLiteAPI.Enum;

namespace JiraLiteAPI.Data
{
    public class WorkTask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TasksStatus Status { get; set; }
        public int ProjectId { get; set; }
        public Priority priority { get; set; }
        public DateOnly Deadline { get; set; }
        public string? AssignedUserId { get; set; }
        public string CreatedBy { get; set; }
        public DateOnly CreatedOn { get; set; }= DateOnly.FromDateTime(DateTime.Now);
        public Project Project { get; set; }
        public ApplicationUser AssignedUser { get; set; }
        public List<Comment> Comments { get; set; }
        public List<ActivityLog> ActivityLogs { get; set; }
        public List<Attachment> Attachments { get; set; }
        public List<TaskRequest>taskRequests { get; set; }





    }
}
