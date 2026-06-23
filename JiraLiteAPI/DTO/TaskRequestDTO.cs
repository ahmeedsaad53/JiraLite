using JiraLiteAPI.Enum;

namespace JiraLiteAPI.DTO
{
    public class TaskRequestDTO
    {
        public int TaskId { get; set; }

    }
    public class MyRequestDTO
    {
        public int Id { get; set; }
        public RequestStatus Status { get; set; }
        public TaskInfoDTO? Task { get; set; }
    }

    public class TaskInfoDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TasksStatus Status { get; set; }
        public DateOnly Deadline { get; set; }
    }
    public class CreateTaskRequestResponseDTO
    {
        public int RequestId { get; set; }
    }
    public class HandleRequestResponseDTO
    {
        public int RequestId { get; set; }
        public RequestStatus Status { get; set; }
    }
    public class TaskRequestItemDTO
    {
        public int Id { get; set; }
        public RequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public TaskMiniDTO? Task { get; set; }
        public UserMiniDTO? User { get; set; }
    }
    public class TaskMiniDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    public class UserMiniDTO
    {
        public string Id { get; set; }
        public string FullName { get; set; }
    }
    public class PaginatedResponse<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<T> Data { get; set; }
    }



}
