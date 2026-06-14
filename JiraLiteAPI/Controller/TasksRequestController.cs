using JiraLiteAPI.Data;
using JiraLiteAPI.DTO;
using JiraLiteAPI.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JiraLiteAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksRequestController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _Context;
        public TasksRequestController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _Context = context;
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTaskRequest(TaskRequestDTO taskRequestDTO)
        {
            if (taskRequestDTO == null) return BadRequest(); 
            var userId=User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();
            var task = await _Context.Tasks.FirstOrDefaultAsync(t => t.Id == taskRequestDTO.TaskId);
            if (task == null) return NotFound();

            
            var isMember = await _Context.ProjectUsers
                .AnyAsync(p => p.UserId == userId&&p.ProjectId==task.ProjectId);
            if (!isMember) return Forbid();
            
            if (task.AssignedUserId != null) return BadRequest("There is a User Already Taked This Task ");
            if(task.Status!= TasksStatus.ToDo) return BadRequest("Task is not available for request");
            var CheakOldRequest=await _Context.TaskRequests.Where(p=>p.TaskId==taskRequestDTO.TaskId&&p.UserId==userId).FirstOrDefaultAsync();
            if (CheakOldRequest != null) return BadRequest("You already Requested this Task before");
           

            var TaskRequests = new TaskRequest
            {
                TaskId = taskRequestDTO.TaskId,
                UserId = userId,
                Status=RequestStatus.pending,
                CreatedAt = DateTime.Now
            };
            await _Context.TaskRequests.AddAsync(TaskRequests);
            await _Context.SaveChangesAsync();
            return Ok("request Sent");

        }




        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRequests(RequestStatus? status,int? taskId,int page = 1,int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize > 50) pageSize = 50;

            var query = _Context.TaskRequests.AsQueryable();

            // Default: Pending if no status
            if (!status.HasValue)
                query = query.Where(r => r.Status == RequestStatus.pending);
            else
                query = query.Where(r => r.Status == status);

            // Filter by task
            if (taskId.HasValue)
                query = query.Where(r => r.TaskId == taskId);

            // Total count
            var totalCount = await query.CountAsync();

            //  Pagination + sorting
            var requests = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    r.Id,
                    r.Status,
                    r.CreatedAt,

                    Task = r.WorkTask == null ? null : new
                    {
                        r.WorkTask.Id,
                        r.WorkTask.Title
                    },

                    User = r.User == null ? null : new
                    {
                        r.User.Id,
                        FullName = (r.User.FName ?? "") + " " + (r.User.LName ?? "")
                    }
                })
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = requests
            });
        }

        [HttpPatch("{requestId:int}")]
        [Authorize (Roles =("Admin"))]
        public async Task<IActionResult> HandleRequest(HandleRequestDTO dto,int requestId )
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
     

            var request = await _Context.TaskRequests
                  .Include(r => r.WorkTask)
                  .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                return NotFound("Request not found");

            if (request.Status != RequestStatus.pending)
                return BadRequest("Request already handled");

            var task = request.WorkTask;

            if (task == null) return NotFound("Task Not Found ");

            if (dto.Status == ApproveRequests.Accepted)
            {
                if(task.AssignedUserId !=null)
                    return BadRequest("Task already assigned");
                task.AssignedUserId = request.UserId;
                task.Status = TasksStatus.InProgress;
                request.Status = RequestStatus.accepted;

                var otherRequests = await _Context.TaskRequests
                            .Where(r => r.TaskId == task.Id && r.Id != requestId)
                            .ToListAsync();

                foreach (var other in otherRequests)
                {
                    other.Status = RequestStatus.rejected;
                }

            }


            else if (dto.Status == ApproveRequests.Rejected)  request.Status = RequestStatus.rejected;
             
            else return BadRequest("Invalid status");
            

            await _Context.SaveChangesAsync();

            return Ok(new
            {
                message = "Request handled successfully",
                requestId = request.Id,
                status = request.Status
            });
        }


        [HttpGet("myTasks")]
        [Authorize]
        public async Task<IActionResult> GetMyRequests()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var requests = await _Context.TaskRequests
             
               .Where(r => r.UserId == userId)
               .Select(r => new
                {
                    r.Id,
                    r.Status,

                    Task = r.WorkTask == null ? null : new
                    {
                        r.WorkTask.Id,
                        Title = r.WorkTask.Title,
                        Description = r.WorkTask.Description,
                        Status = r.WorkTask.Status,
                        Deadline = r.WorkTask.Deadline 
                     }
                })
                .ToListAsync();

            return Ok(requests);
        }

        [HttpDelete("{RequestId:int}")]
        [Authorize(Roles = ("Admin"))]
        public async Task<IActionResult> DeleteRequest(int RequestId)
        {
            var RequestsTask = await _Context.TaskRequests.FirstOrDefaultAsync(t => t.Id == RequestId);
            if (RequestsTask == null) return NotFound();
            _Context.TaskRequests.Remove(RequestsTask);
            await _Context.SaveChangesAsync();
            return Ok(new
            {
                message = "Request deleted successfully",
                RequestsTask = RequestId
            });


        }




    }
}
