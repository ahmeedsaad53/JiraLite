namespace JiraLiteAPI.DTO
{
    public class CommentDTO
    {
        public string Content { get; set; }
        public int TaskId { get; set; }
        public string UserId { get; set; }
        public int ProjectId { get; set; }
        public DateTime CreatedAt { get; set; }= DateTime.Now;
    }
}
