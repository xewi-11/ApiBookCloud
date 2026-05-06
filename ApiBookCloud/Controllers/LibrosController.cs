using ApiBookCloud.Data;
using ApiBookCloud.Models;
using BookCloud.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NugetModelsBookCloud.Models;

namespace ApiBookCloud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibrosController : ControllerBase
    {
        private readonly RepositoryLibros repo;
        private readonly BookCloudContext context;

        public LibrosController(RepositoryLibros repo, BookCloudContext context)
        {
            this.repo = repo;
            this.context = context;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<List<Libro>>> GetLibros()
        {
            var libros = await this.repo.GetLibros();
            return Ok(libros);
        }

        [HttpGet]
        [Route("[action]/{id:int}")]
        public async Task<ActionResult<Libro>> GetLibro(int id)
        {
            var libro = await this.repo.GetLibro(id);
            if (libro == null)
            {
                return NotFound("Libro no encontrado.");
            }
            return Ok(libro);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> InsertLibro([FromForm] CreateLibroRequestDto request)
        {
            Stream? stream = null;
            string? fileName = null;

            if (request.Foto != null)
            {
                stream = request.Foto.OpenReadStream();
                fileName = request.Foto.FileName;
            }

            Libro libro = new Libro
            {
                Titulo = request.Titulo,
                Autor = request.Autor,
                Descripcion = request.Descripcion,
                Precio = request.Precio,
                Stock = request.Stock,
                UsuarioId = request.UsuarioId,
                FechaPublicacion = request.FechaPublicacion,
                Activo = request.Activo
            };

            int id = await this.repo.InsertLibro(libro, stream, fileName);
            return Ok(new { Id = id });
        }

        [HttpPut]
        [Route("[action]")]
        public async Task<ActionResult> UpdateLibro([FromForm] UpdateLibroRequestDto request)
        {
            Stream? stream = null;
            string? fileName = null;

            if (request.FotoNueva != null)
            {
                stream = request.FotoNueva.OpenReadStream();
                fileName = request.FotoNueva.FileName;
            }

            Libro libro = new Libro
            {
                Id = request.Id,
                Titulo = request.Titulo,
                Autor = request.Autor,
                Descripcion = request.Descripcion,
                Precio = request.Precio,
                Stock = request.Stock,
                UsuarioId = request.UsuarioId,
                FechaPublicacion = request.FechaPublicacion,
                Activo = request.Activo,
                Foto = request.Foto
            };

            await this.repo.UpdateLibro(libro, stream, fileName);
            return Ok();
        }

        [HttpDelete]
        [Route("[action]/{id:int}")]
        public async Task<ActionResult> DeleteLibro(int id)
        {
            await this.repo.DeleteLibro(id);
            return Ok();
        }
    }
}
