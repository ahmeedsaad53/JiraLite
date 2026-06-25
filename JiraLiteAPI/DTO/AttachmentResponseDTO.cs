namespace JiraLiteAPI.DTO
{
    public class AttachmentResponseDTO
    {

        public int Id { get; set; }

        public string FileName { get; set; }

        public string FileUrl { get; set; }

        public DateTime UploadAt { get; set; }

        public string? UploadedByName { get; set; }

    }
}
