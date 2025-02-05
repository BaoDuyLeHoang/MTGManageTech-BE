﻿using MartyrGraveManagement_BAL.ModelViews.AssignmentTaskFeedbackDTOs;
using MartyrGraveManagement_BAL.ModelViews.RequestFeedbackDTOs;
using MartyrGraveManagement_BAL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MartyrGraveManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestFeedbackController : ControllerBase
    {
        private readonly IRequestFeedbackService _feedbackService;
        private readonly IAuthorizeService _authorizeService;

        public RequestFeedbackController(IRequestFeedbackService feedbackService, IAuthorizeService authorizeService)
        {
            _feedbackService = feedbackService;
            _authorizeService = authorizeService;
        }

        /// <summary>
        /// Create Feedback after Order Completed (Customer Role)
        /// </summary>
        [Authorize(Policy = "RequireCustomerRole")]
        [HttpPost("Create-Feedback")]
        public async Task<IActionResult> CreateFeedback([FromBody] RequestFeedbackDtoRequest feedbackDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Lấy accountId từ token
            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdClaim))
            {
                return Forbid("Token does not contain accountId.");
            }

            int accountIdFromToken = int.Parse(accountIdClaim);

            // Kiểm tra accountId từ token có khớp với accountId từ request không
            if (accountIdFromToken != feedbackDto.CustomerId)
            {
                return Forbid("You can only create feedback for your own recurring services.");
            }

            var result = await _feedbackService.CreateFeedbackAsync(feedbackDto);
            if (!result.success)
            {
                return BadRequest(result.message);
            }

            return Ok(result.feedback);
        }

        /// <summary>
        /// Staff response feedback
        /// </summary>    
        [Authorize(Policy = "RequireStaffRole")]
        [HttpPost("Create-Feedback-Response")]
        public async Task<IActionResult> CreateFeedbackResponse([FromBody] RequestFeedbackResponseDtoRequest feedbackDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Lấy StaffId từ token
            var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(staffIdClaim))
            {
                return Forbid("Token không chứa StaffId.");
            }

            feedbackDto.StaffId = int.Parse(staffIdClaim); // Gán StaffId từ token

            var result = await _feedbackService.CreateFeedbackResponseAsync(feedbackDto);
            if (!result.success)
            {
                return BadRequest(result.message);
            }

            return Ok(result.message);
        }


        /// <summary>
        /// get feedback by Id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeedbackById(int id)
        {
            var result = await _feedbackService.GetFeedbackByIdAsync(id);

            if (!result.success)
            {
                return NotFound(result.message);
            }

            return Ok(result.feedback);  // Trả về đối tượng feedback đã chứa FullName và CustomerCode
        }

        /// <summary>
        /// get feedback by RequestId
        /// </summary>
        [HttpGet("getFeedbackWithRequestId/{requestId}")]
        public async Task<IActionResult> GetFeedbackByRequestId(int requestId)
        {
            var result = await _feedbackService.GetFeedbackByRequestId(requestId);

            if (!result.success)
            {
                return NotFound(result.message);
            }

            return Ok(result.feedback);  // Trả về đối tượng feedback đã chứa FullName và CustomerCode
        }


        /// <summary>
        /// get all feedback (Staff or Manager Role)
        /// </summary>
        [Authorize(Policy = "RequireManagerOrStaffRole")]
        [HttpGet("Get-all-feedback")]
        public async Task<IActionResult> GetAllFeedbacks([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _feedbackService.GetAllFeedbacksAsync(page, pageSize);
            if (!result.success)
            {
                return BadRequest(result.message);
            }

            return Ok(new { feedbacks = result.feedbacks, totalPage = result.totalPage });
        }

        /// <summary>
        /// Update feedback content (Customer role)
        /// </summary>
        [Authorize(Policy = "RequireCustomerRole")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFeedback(int id, [FromBody] AssignmentTaskFeedbackContentDtoRequest feedbackDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customerId = User.FindFirst("AccountId")?.Value;
            if (customerId == null)
            {
                return Forbid();
            }

            var result = await _feedbackService.UpdateFeedbackAsync(id, int.Parse(customerId), feedbackDto);
            if (!result.success)
            {
                return NotFound(result.message);
            }

            return Ok(result.message);
        }

        /// <summary>
        /// Update feedback status (ManagerOrStaffRole)
        /// </summary>       
        [Authorize(Policy = "RequireManagerOrStaffRole")]  // Chỉ cho phép Admin hoặc Manager thay đổi trạng thái
        [HttpPut("{id}/change-status")]
        public async Task<IActionResult> ChangeStatusFeedback(int id)
        {
            var result = await _feedbackService.ChangeStatusFeedbackAsync(id);
            if (!result.success)
            {
                return BadRequest(result.message);
            }

            return Ok(result.message);
        }



        /// <summary>
        /// Delete Feedback 
        /// </summary>
        [Authorize(Policy = "RequireManagerOrStaffRole")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var result = await _feedbackService.DeleteFeedbackAsync(id);
            if (!result.success)
            {
                return NotFound(result.message);
            }

            return Ok(result.message);
        }

        /// <summary>
        /// Staff update response feeback
        /// </summary>    
        [Authorize(Policy = "RequireStaffRole")]
        [HttpPut("Update-Feedback-Response/{feedbackId}")]
        public async Task<IActionResult> UpdateFeedbackResponse(int feedbackId, [FromBody] RequestFeedbackResponseDtoRequest feedbackDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Lấy StaffId từ token để xác thực quyền của nhân viên
            var staffIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(staffIdClaim))
            {
                return Forbid("Token does not contain StaffId.");
            }

            // Gán StaffId từ token vào DTO để đảm bảo an toàn
            feedbackDto.StaffId = int.Parse(staffIdClaim);

            // Gọi phương thức trong FeedbackService để cập nhật nội dung phản hồi
            var result = await _feedbackService.UpdateFeedbackResponseAsync(feedbackDto);

            if (!result.success)
            {
                return BadRequest(result.message);
            }

            return Ok(result.message);
        }
    }
}
