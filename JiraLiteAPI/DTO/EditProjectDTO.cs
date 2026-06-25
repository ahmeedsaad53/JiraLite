using JiraLiteAPI.Enum;

namespace JiraLiteAPI.DTO
{
    public class EditProjectDTO
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public DateOnly DeadLine { get; set; }

        public ProjectStatus Status { get; set; }
    }
}