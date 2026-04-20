using Azure;
using Microsoft.AspNetCore.Mvc;
using MvcCoreAzureStorage.Services;

namespace ApiBookCloud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly RepositoryStorageBlobs storage;

        public MediaController(RepositoryStorageBlobs storage)
        {
            this.storage = storage;
        }

        [HttpGet("usuarios/{blobName}")]
        public async Task<IActionResult> GetUsuarioFoto(string blobName)
        {
            try
            {
                var (stream, contentType) = await this.storage.GetBlobStreamAsync(blobName, "usuarios");
                return File(stream, contentType);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return NotFound();
            }
        }

        [HttpGet("libros/{blobName}")]
        public async Task<IActionResult> GetLibroFoto(string blobName)
        {
            try
            {
                var (stream, contentType) = await this.storage.GetBlobStreamAsync(blobName, "libros");
                return File(stream, contentType);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return NotFound();
            }
        }
    }
}
