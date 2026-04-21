using ApiBookCloud.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NugetModelsBookCloud.Models;

namespace ApiBookCloud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly RepositoryChats _repository;

        public ChatsController(RepositoryChats repository)
        {
            _repository = repository;
        }

        [Authorize]
        [HttpGet("iniciar/{usuario1Id}/{usuario2Id}")]
        public async Task<ActionResult<Chat>> ObtenerOCrearChat(int usuario1Id, int usuario2Id)
        {
            var chat = await _repository.ObtenerOCrearChatAsync(usuario1Id, usuario2Id);
            return Ok(chat);
        }

        [Authorize]
        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<List<Chat>>> ObtenerChatsDeUsuario(int usuarioId)
        {
            var chats = await _repository.ObtenerChatsDeUsuarioAsync(usuarioId);
            return Ok(chats);
        }

        [Authorize]
        [HttpGet("{chatId}")]
        public async Task<ActionResult<Chat>> ObtenerChatPorId(int chatId)
        {
            var chat = await _repository.ObtenerChatPorIdAsync(chatId);
            if (chat == null) return NotFound();
            return Ok(chat);
        }

        [Authorize]
        [HttpGet("{chatId}/mensajes")]
        public async Task<ActionResult<List<Mensaje>>> ObtenerMensajesDelChat(int chatId, [FromQuery] int take = 50)
        {
            var mensajes = await _repository.ObtenerMensajesDelChatAsync(chatId, take);
            return Ok(mensajes);
        }

        [Authorize]
        [HttpPost("{chatId}/mensaje")]
        public async Task<ActionResult<Mensaje>> EnviarMensaje(int chatId, [FromQuery] int remitenteId, [FromBody] string contenido)
        {
            var mensaje = await _repository.EnviarMensajeAsync(chatId, remitenteId, contenido);
            return Ok(mensaje);
        }

        [Authorize]
        [HttpPost("{chatId}/ubicacion")]
        public async Task<ActionResult<Mensaje>> EnviarUbicacion(int chatId, [FromQuery] int remitenteId, [FromQuery] decimal latitud, [FromQuery] decimal longitud)
        {
            var mensaje = await _repository.EnviarUbicacionAsync(chatId, remitenteId, latitud, longitud);
            return Ok(mensaje);
        }

        [Authorize]
        [HttpGet("pertenece/{chatId}/{usuarioId}")]
        public async Task<ActionResult<bool>> UsuarioPerteneceChat(int chatId, int usuarioId)
        {
            var pertenece = await _repository.UsuarioPerteneceChatAsync(chatId, usuarioId);
            return Ok(pertenece);
        }
    }
}