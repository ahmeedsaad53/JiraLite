using Microsoft.AspNetCore.Mvc;
using JiraLiteAPI.DTO.Common;

namespace JiraLiteAPI.Controller
{
    public class BaseController : ControllerBase
    {
        protected IActionResult HandleResponse<T>(ServiceResponse<T> result)
        {
            if (!result.Success)
            {
                if (result.Message.Contains("Unauthorized"))
                    return Unauthorized(result);

                if (result.Message.Contains("Forbidden"))
                    return Forbid();

                if (result.Message.Contains("not found"))
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}