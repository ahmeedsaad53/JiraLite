using JiraLiteAPI.Enum;

namespace JiraLiteAPI.DTO
{
    public class TaskDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int ProjectId { get; set; }
        public Priority priority { get; set; }
        public DateOnly Deadline { get; set; }
        public bool IsHard { get; set; }
        public string? AssignedUserId { get; set; }

    }
}
