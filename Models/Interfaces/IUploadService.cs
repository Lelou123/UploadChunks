using FluentResults;

namespace UploadChunks.Models.Interfaces
{
    public interface IUploadService
    {
        Task<Result> UploadFileChunks(IFormFile file);
    }
}
