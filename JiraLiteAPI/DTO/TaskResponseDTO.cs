using JiraLiteAPI.Enum;

namespace JiraLiteAPI.DTO
{
    public class TaskResponseDTO
    {

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TasksStatus Status { get; set; }
        public Priority Priority { get; set; }
        public DateOnly Deadline { get; set; }
        public int ProjectId { get; set; }
        public string? AssignedUserName { get; set; }

    }
}
