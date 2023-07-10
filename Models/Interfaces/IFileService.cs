using FluentResults;

namespace UploadChunks.Models.Interfaces
{
    public interface IFileService
    {
        Task<Result> CombineFileChunks(string directoryPath, string fileName, string outputPath);
        Task<Result> UploadFileChunks(IFormFile file, string basePath, string fileName);
    }
}
