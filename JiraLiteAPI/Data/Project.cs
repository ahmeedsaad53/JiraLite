using JiraLiteAPI.Enum;

namespace JiraLiteAPI.Data
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateOnly CreatedOn { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public DateTime DeadLine { get; set; }  
        public ProjectStatus Status { get; set; }
        public List<ProjectUser> Users { get; set; }
        public List<WorkTask> Tasks { get; set; }
       

    }
}
