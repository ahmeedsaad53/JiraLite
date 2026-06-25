namespace JiraLiteAPI.DTO
{
    public class DownloadFileDTO
    {

        public byte[] FileBytes { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }

    }
}
