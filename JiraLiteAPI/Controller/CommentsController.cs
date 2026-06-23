using Humanizer;
using JiraLiteAPI.Data.Context;
using JiraLiteAPI.Data.Models;
using JiraLiteAPI.DTO;
using JiraLiteAPI.Enum;
using JiraLiteAPI.Service.CommentSernice;
using JiraLiteAPI.Service.PService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using System.Security.Claims;

namespace JiraLiteAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {

        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        [Authorize]
       
         
        public async Task<IActionResult> MakeANewComment(CommentDTO commentDTO)
        {
            if (!ModelState.IsValid)//cheak the data of taskdto
                return BadRequest(ModelState);
            var result = await _commentService.MakeANewComment(commentDTO, User);
            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetComments(int? taskId, int page = 1, int pageSize = 10)
        {

            if (!ModelState.IsValid)//cheak the data of taskdto
                return BadRequest(ModelState);
            var result=await _commentService.GetComments(taskId,User,pageSize,page);
            return Ok(result);
           
        }
        [HttpDelete("{CommentId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int CommentId)

        {
            if (!ModelState.IsValid)//cheak the data of taskdto
                return BadRequest(ModelState);
            var result= await _commentService.DeleteComment(CommentId,User);
            return Ok(result);


        }
        [HttpGet("myComment")]
        [Authorize]
        public async Task<IActionResult> GetMyComment()
        {
            if (!ModelState.IsValid)//cheak the data of taskdto
                return BadRequest(ModelState);
            var result = await _commentService.GetMyComment( User);
            return Ok(result);

        }


        [HttpPatch("{CommentId:int}")]
        [Authorize]
        public async Task<IActionResult> EditComment(int CommentId, EditCommentDTO dto)
        {
            if (!ModelState.IsValid)//cheak the data of taskdto
                return BadRequest(ModelState);
            var result = await _commentService.EditComment(CommentId, dto, User);
            return Ok(result);

        }














    }
}
