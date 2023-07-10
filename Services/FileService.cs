using FluentResults;
using System.Text.RegularExpressions;
using UploadChunks.Models.Interfaces;

namespace UploadChunks.Services
{
    public class FileService : IFileService
    {
        public async Task<Result> UploadFileChunks(IFormFile file, string basePath, string fileName)
        {
            if (file == null || file.Length == 0)
                return Result.Fail("No file selected.");

            try
            {
                var directoryPath = Path.Combine(basePath, Path.GetDirectoryName(fileName));

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                var filePath = Path.Combine(directoryPath, Path.GetFileName(fileName));

                if (File.Exists(filePath))
                    File.Delete(filePath);

                int maxBlockSize = 4096; // Definir o tamanho máximo de cada bloco (chunk)

                using (var fileStream = file.OpenReadStream())
                {
                    byte[] buffer = new byte[maxBlockSize];
                    int bytesRead;
                    int blockNumber = 1;

                    while ((bytesRead = await fileStream.ReadAsync(buffer, 0, maxBlockSize)) > 0)
                    {
                        var chunkFilePath = Path.Combine(directoryPath, $"{fileName}.part{blockNumber}");

                        using (var chunkStream = new FileStream(chunkFilePath, FileMode.Create))
                        {
                            await chunkStream.WriteAsync(buffer, 0, bytesRead);
                        }

                        blockNumber++;
                    }
                }

                return Result.Ok().WithSuccess(directoryPath);
            }
            catch (Exception ex)
            {
                return Result.Fail($"Failed to upload file chunks: {ex.Message}");
            }
        }

        public async Task<Result> CombineFileChunks(string directoryPath, string fileName, string outputPath)
        {
            try
            {
                var chunkFiles = Directory.GetFiles(directoryPath, $"{fileName}.part*");

                // Função de ordenação personalizada para ordenar numericamente os arquivos de chunk
                var orderedChunkFiles = chunkFiles.OrderBy(file =>
                {
                    var match = Regex.Match(file, $@"{Regex.Escape(fileName)}\.part(\d+)$");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int number))
                        return number;
                    return 0;
                });

                // Verificar a integridade do arquivo de saída antes de excluir os chunks
                long expectedFileSize = orderedChunkFiles.Sum(chunkFile => new FileInfo(chunkFile).Length);

                using (var outputStream = new FileStream(outputPath, FileMode.Create))
                {
                    foreach (var chunkFile in orderedChunkFiles)
                    {
                        using (var chunkStream = new FileStream(chunkFile, FileMode.Open))
                        {
                            await chunkStream.CopyToAsync(outputStream);
                        }
                    }
                }

                // Remover os arquivos de chunk após a combinação
                foreach (var chunkFile in orderedChunkFiles)
                {
                    File.Delete(chunkFile);
                }

                // Verificar a integridade do arquivo de saída após a combinação
                using (var outputFileStream = new FileStream(outputPath, FileMode.Open))
                {
                    var actualFileSize = outputFileStream.Length;

                    if (expectedFileSize != actualFileSize)
                    {
                        File.Delete(outputPath);
                        return Result.Fail("Failed to combine file chunks: Unexpected file size. File may be corrupted.");
                    }
                }

                return Result.Ok().WithSuccess(outputPath);
            }
            catch (Exception ex)
            {
                return Result.Fail($"Failed to combine file chunks: {ex.Message}");
            }
        }





    }
}
