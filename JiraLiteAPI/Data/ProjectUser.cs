using JiraLiteAPI.Enum;

namespace JiraLiteAPI.Data
{
    public class ProjectUser
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ProjectId { get; set; }
        public ApplicationUser User { get; set; }
        public Project Project { get; set; }

    }
}
