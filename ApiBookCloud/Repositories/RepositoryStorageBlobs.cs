using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ApiBookCloud.Models;
using System.Threading;

namespace MvcCoreAzureStorage.Services
{
    public class RepositoryStorageBlobs
    {
        private readonly BlobServiceClient client;
        private readonly string containerName;

        public RepositoryStorageBlobs(BlobServiceClient client, IConfiguration configuration)
        {
            this.client = client;
            this.containerName = configuration.GetValue<string>("AzureStorage:ContainerName")
                ?? throw new ArgumentNullException("AzureStorage:ContainerName", "Debes configurar AzureStorage:ContainerName en appsettings.");
        }

        public async Task<List<BlobModel>> GetBlobsAsync(string? virtualDirectory = null)
        {
            BlobContainerClient containerClient = this.GetContainerClient();
            string? prefix = NormalizeDirectory(virtualDirectory);

            List<BlobModel> models = new();
            await foreach (BlobItem item in containerClient.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix, CancellationToken.None))
            {
                if (item.Name.EndsWith(".init", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                BlobClient blobClient = containerClient.GetBlobClient(item.Name);
                models.Add(new BlobModel
                {
                    Nombre = item.Name,
                    Container = this.containerName,
                    Url = blobClient.Uri.AbsoluteUri
                });
            }

            return models;
        }

        public async Task DeleteBlobAsync(string blobName, string? virtualDirectory = null)
        {
            BlobContainerClient containerClient = this.GetContainerClient();
            string blobPath = BuildBlobPath(virtualDirectory, blobName);
            await containerClient.DeleteBlobIfExistsAsync(blobPath);
        }

        public async Task<string> UploadloadAsync(string blobName, Stream stream, string? virtualDirectory = null)
        {
            BlobContainerClient containerClient = this.GetContainerClient();

            if (!string.IsNullOrWhiteSpace(virtualDirectory))
            {
                await EnsureVirtualDirectoryAsync(containerClient, virtualDirectory);
            }

            string blobPath = BuildBlobPath(virtualDirectory, blobName);
            BlobClient blobClient = containerClient.GetBlobClient(blobPath);
            await blobClient.UploadAsync(stream, overwrite: true);

            // Devolvemos la ruta/nombre del blob para persistirla en BD,
            // no la URL absoluta (contenedor privado).
            return blobPath;
        }

        public async Task<(Stream Stream, string ContentType)> GetBlobStreamAsync(string blobName, string? virtualDirectory = null)
        {
            BlobContainerClient containerClient = this.GetContainerClient();
            string blobPath = BuildBlobPath(virtualDirectory, blobName);

            BlobClient blobClient = containerClient.GetBlobClient(blobPath);
            BlobDownloadInfo download = await blobClient.DownloadAsync();
            string contentType = download.ContentType ?? "application/octet-stream";
            return (download.Content, contentType);
        }

        private BlobContainerClient GetContainerClient()
        {
            return this.client.GetBlobContainerClient(this.containerName.ToLowerInvariant());
        }

        private static async Task EnsureVirtualDirectoryAsync(BlobContainerClient containerClient, string virtualDirectory)
        {
            string directory = NormalizeDirectory(virtualDirectory)!;
            BlobClient markerBlob = containerClient.GetBlobClient($"{directory}.init");

            if (!await markerBlob.ExistsAsync())
            {
                using MemoryStream empty = new();
                await markerBlob.UploadAsync(empty);
            }
        }

        private static string BuildBlobPath(string? virtualDirectory, string blobName)
        {
            string normalizedBlob = blobName.TrimStart('/').Trim();
            string? directory = NormalizeDirectory(virtualDirectory);

            if (string.IsNullOrWhiteSpace(directory))
            {
                return normalizedBlob;
            }

            return $"{directory}{normalizedBlob}";
        }

        private static string? NormalizeDirectory(string? virtualDirectory)
        {
            if (string.IsNullOrWhiteSpace(virtualDirectory))
            {
                return null;
            }

            string directory = virtualDirectory.Trim().Trim('/');
            return string.IsNullOrWhiteSpace(directory) ? null : $"{directory}/";
        }
    }
}
