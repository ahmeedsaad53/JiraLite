namespace JiraLiteAPI.DTO
{
    public class ActivityLogResponseDTO
    {
        public int Id { get; set; }

        public int TaskId { get; set; }

        public string Action { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? UserFullName { get; set; }
    }   
}
