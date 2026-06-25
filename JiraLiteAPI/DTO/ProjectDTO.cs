using JiraLiteAPI.Enum;

namespace JiraLiteAPI.DTO
{
    public class ProjectDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateOnly DeadLine { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public ProjectStatus Status { get; set; }
    }


        public class ProjectResponseDTO
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public string CreatedByName { get; set; }

            public DateOnly DeadLine { get; set; }

            public DateTime CreatedOn { get; set; }

            public ProjectStatus Status { get; set; }
        }

    public class MessageDTO
    {
        public string Message { get; set; }
    }

}


