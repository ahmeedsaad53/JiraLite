using JiraLiteAPI.Enum;

namespace JiraLiteAPI.Data
{
    public class TaskRequest
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string UserId { get; set; }
        public RequestStatus Status { get; set; }
        public ApplicationUser User { get; set; }
        public WorkTask WorkTask { get; set; }
    }
}
