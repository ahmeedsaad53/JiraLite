using JiraLiteAPI.Enum;
namespace JiraLiteAPI.DTO
{
    public class HandleRequestDTO
    {
        public int TaskId {  get; set; }
        public ApproveRequests Status {  get; set; }
    }
}
