using JiraLiteAPI.Data.Context;

namespace JiraLiteAPI.Data.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int TaskId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; } 
        public ApplicationUser User { get; set; }
        public WorkTask Task { get; set; }

    }
}
