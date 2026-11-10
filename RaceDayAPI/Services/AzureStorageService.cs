
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDayAPI.Services
{
    public class AzureStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureStorageService(IConfiguration configuration)
        {
            string connectionString = configuration
                .GetConnectionString("AzureStorage")
                ?? configuration["AzureStorage:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");

            _blobServiceClient =
                new BlobServiceClient(connectionString);
        }

        // UPLOAD FILE
        public async Task<string> UploadFileAsync(
            IFormFile file,
            string containerName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Please select a file.");

            var containerClient =
                _blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync(
                PublicAccessType.None);

            string fileName =
                $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            var blobClient =
                containerClient.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();

            await blobClient.UploadAsync(
                stream,
                new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                });

            return blobClient.Uri.ToString();
        }

        // DOWNLOAD FILE FROM PRIVATE AZURE STORAGE
        public async Task<(Stream Content, string ContentType)>
            DownloadFileAsync(string blobUri)
        {
            if (!Uri.TryCreate(
                blobUri,
                UriKind.Absolute,
                out Uri? uri))
            {
                throw new ArgumentException("Invalid image URL.");
            }

            string[] parts =
                uri.AbsolutePath.Trim('/').Split('/', 2);

            if (parts.Length != 2)
                throw new ArgumentException("Invalid Azure blob URL.");

            string containerName =
                Uri.UnescapeDataString(parts[0]);

            string blobName =
                Uri.UnescapeDataString(parts[1]);

            var containerClient =
                _blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient =
                containerClient.GetBlobClient(blobName);

            var download =
                await blobClient.DownloadStreamingAsync();

            return (
                download.Value.Content,
                download.Value.Details.ContentType
                    ?? "application/octet-stream"
            );
        }
    }
}
