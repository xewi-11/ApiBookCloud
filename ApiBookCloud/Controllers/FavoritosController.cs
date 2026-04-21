using ApiBookCloud.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NugetModelsBookCloud.Models;

namespace ApiBookCloud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritosController : ControllerBase
    {
        private readonly RepositoryFavoritos _repository;

        public FavoritosController(RepositoryFavoritos repository)
        {
            _repository = repository;
        }

        [Authorize]
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<List<Libro>>> GetFavoritosUsuario(int usuarioId)
        {
            var favoritos = await _repository.GetFavoritosByUsuarioAsync(usuarioId);
            return Ok(favoritos);
        }

        [Authorize]
        [HttpPost("add/{usuarioId}/{libroId}")]
        public async Task<ActionResult> AddFavorito(int usuarioId, int libroId)
        {
            await _repository.AddFavoritoAsync(usuarioId, libroId);
            return Ok();
        }

        [Authorize]
        [HttpDelete("remove/{usuarioId}/{libroId}")]
        public async Task<ActionResult> RemoveFavorito(int usuarioId, int libroId)
        {
            await _repository.RemoveFavoritoAsync(usuarioId, libroId);
            return Ok();
        }
    }
}