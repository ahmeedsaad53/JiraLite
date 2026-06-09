using JiraLiteAPI.Enum;

namespace JiraLiteAPI.DTO
{
    public class ProjectDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateOnly CreatedOn { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public DateOnly DeadLine { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public ProjectStatus Status { get; set; }
    }
}
