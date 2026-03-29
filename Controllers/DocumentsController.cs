using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sehha360.Models.ApiResponse;
using Sehha360.Services.Interface;
using System.Security.Claims;

namespace Sehha360.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(20 * 1024 * 1024)] // 20 MB
        public async Task<IActionResult> UploadDocument(IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized(ApiResponse.FaliureResponse("Unauthorized"));

            var response = await _documentService.UploadDocumentAsync(file, userId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpGet("{id}/url")]
        public async Task<IActionResult> GetDocumentUrl(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized(ApiResponse.FaliureResponse("Unauthorized"));

            var response = await _documentService.GetDocumentUrlAsync(id, userId);
            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
    }
}
