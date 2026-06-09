using JiraLiteAPI.Enum;

namespace JiraLiteAPI.DTO
{
    public class UpdateProjectProgressDTO
    {
        public ProjectStatus ProjectStatus { get; set; }
        public string Description { get; set; }
    }
}
