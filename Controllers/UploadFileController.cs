using Microsoft.AspNetCore.Mvc;
using UploadChunks.Models.Interfaces;

namespace UploadChunks.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UploadFileController : ControllerBase
    {
        private readonly IUploadService _uploadService;

        public UploadFileController(IUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        [HttpPost]
        [Route("/Chunks")]
        public async Task<IActionResult> UploadChunkFile(IFormFile file)
        {
            var result = await _uploadService.UploadFileChunks(file);

            if (result.IsFailed)
                return BadRequest(result.Errors.FirstOrDefault());


            return Ok(result.Successes.FirstOrDefault());
        }
    }
}
