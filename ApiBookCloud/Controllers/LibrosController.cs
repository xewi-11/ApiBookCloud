using ApiBookCloud.Data;
using BookCloud.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NugetModelsBookCloud.Models;
using NugetModelsBookCloud.Repositories.Interfaces;

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
        public async Task<ActionResult> InsertLibro([FromForm] Libro libro, IFormFile? foto)
        {
            Stream? stream = null;
            string? fileName = null;

            if (foto != null)
            {
                stream = foto.OpenReadStream();
                fileName = foto.FileName;
            }

            int id = await this.repo.InsertLibro(libro, stream, fileName);
            return Ok(new { Id = id });
        }

        [HttpPut]
        [Route("[action]")]
        public async Task<ActionResult> UpdateLibro([FromForm] Libro libro, IFormFile? foto)
        {
            Stream? stream = null;
            string? fileName = null;

            if (foto != null)
            {
                stream = foto.OpenReadStream();
                fileName = foto.FileName;
            }

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
