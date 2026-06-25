namespace JiraLiteAPI.DTO
{
    public class CommentResponseDTO
    {

        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? UserFullName { get; set; }

        public int TaskId { get; set; }
        public string? TaskTitle { get; set; }

    }
}
