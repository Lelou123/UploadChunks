using FluentResults;
using UploadChunks.Models.Interfaces;

namespace UploadChunks.Services
{
    public class UploadService : IUploadService
    {
        private readonly IFileService _fileService;
        private readonly IWebHostEnvironment _env;

        public UploadService(IWebHostEnvironment env, IFileService fileService)
        {
            _env = env;
            _fileService = fileService;
        }

        public async Task<Result> UploadFileChunks(IFormFile file)
        {

            var basePath = Path.Combine(_env.ContentRootPath, "Files");

            var uploadResult = await _fileService.UploadFileChunks(file, basePath, file.FileName);


            if (uploadResult.IsFailed)
                return Result.Fail("File Upload Failed");


           
            var outputPath = Path.Combine(basePath, file.FileName);

            var combineResult = await _fileService.CombineFileChunks(basePath, file.FileName, outputPath);
            
            if (combineResult.IsFailed)
                return Result.Fail(combineResult.Errors.FirstOrDefault());


            return Result.Ok().WithSuccess("File Uploaded with Success");
        }
    }
}
