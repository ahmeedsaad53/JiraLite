namespace JiraLiteAPI.Data
{
    public class Attachment
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string UploadedByUserId { get; set; }
        public DateOnly UploadAt { get; set; }
        public WorkTask Task { get; set; }



    }
}
