using ApiBookCloud.Models;
using ApiBookCloud.Repositories;
using ApiOAuthEmpleados.Helpers;
using BookCloud.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcCoreAzureStorage.Services;
using NugetModelsBookCloud.Models;

namespace ApiBookCloud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsarioController : ControllerBase
    {
        private readonly RepositoryUsuarios repo;
        private readonly HelperUsuarioToken helper;
        private readonly RepositoryStorageBlobs storage;

        public UsarioController(RepositoryUsuarios repo, HelperUsuarioToken helper, RepositoryStorageBlobs storage)
        {
            this.repo = repo;
            this.helper = helper;
            this.storage = storage;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult> GetInfoUsuario()
        {
            int? userId = this.helper.GetUserId();
            if (userId is null)
            {
                return Unauthorized("No se pudo obtener el ID del usuario.");
            }

            Usuario? user = await this.repo.GetInfoUsario(userId.Value.ToString());
            if (user is null)
            {
                return NotFound("Usuario no encontrado.");
            }

            return Ok(new
            {
                user.Id,
                user.Nombre,
                Correo = user.Correo,
                user.FechaRegistro,
                user.Activo,
                Foto = user.Foto
            });
        }

        [HttpGet]
        [Route("[action]/{id:int}")]
        public async Task<ActionResult> GetUsuarioById(int id)
        {
            Usuario? user = await this.repo.GetUserById(id);
            if (user is null)
            {
                return NotFound("Usuario no encontrado.");
            }

            return Ok(new
            {
                user.Id,
                user.Nombre,
                Correo = user.Correo,
                user.FechaRegistro,
                user.Activo,
                Foto = user.Foto
            });
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult> GetUsuarioByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("Debes indicar un email.");
            }

            Usuario? user = await this.repo.GetUserByEmail(email);
            if (user is null)
            {
                return NotFound("Usuario no encontrado.");
            }

            return Ok(new
            {
                user.Id,
                user.Nombre,
                Correo = user.Correo,
                user.FechaRegistro,
                user.Activo,
                Foto = user.Foto
            });
        }

        [HttpGet]
        [Route("[action]/{idUsuario:int}")]
        public async Task<ActionResult> GetSeguridadUsuario(int idUsuario)
        {
            UsuarioSeguridad? seguridad = await this.repo.GetSeguridadUsuario(idUsuario);
            if (seguridad is null)
            {
                return NotFound("Seguridad de usuario no encontrada.");
            }

            return Ok(new
            {
                seguridad.Id,
                seguridad.UsuarioId,
                seguridad.Activo
            });
        }

        [HttpPut]
        [Route("[action]/{id:int}")]
        public async Task<ActionResult> ActualizarUsuario(int id, [FromForm] UpdateUsuarioDto dto, IFormFile? foto)
        {
            if (dto is null)
            {
                return BadRequest("Datos de usuario inválidos.");
            }

            Usuario? user = await this.repo.GetInfoUsario(id.ToString());
            if (user is null)
            {
                return NotFound("Usuario no encontrado.");
            }

            user.Nombre = dto.Nombre;
            user.Correo = dto.Correo;
            user.Activo = dto.Activo;

            if (foto is not null && foto.Length > 0)
            {
                string extension = Path.GetExtension(foto.FileName);
                string blobName = $"usuario_{id}_{Guid.NewGuid():N}{extension}";

                using Stream stream = foto.OpenReadStream();
                await this.storage.UploadloadAsync(blobName, stream, "usuarios");
                user.Foto = blobName;
            }
            else
            {
                user.Foto = dto.Foto;
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.Password = dto.Password;

                UsuarioSeguridad? seguridad = await this.repo.GetSeguridadUsuario(user.Id);
                if (seguridad is null)
                {
                    return NotFound("Seguridad de usuario no encontrada para actualizar la contraseña.");
                }

                string salt = EncryptionPassword.GenerateSalt();
                byte[] hash = EncryptionPassword.EncryptPassword(dto.Password, salt);

                seguridad.PasswordHash = hash;
                seguridad.Salt = salt;

                await this.repo.ActualizarSeguridadUsuarioAsync(seguridad);
            }

            await this.repo.ActualizarUsuarioAsync(user);
            return NoContent();
        }

        [HttpPut]
        [Route("[action]/{id:int}")]
        public async Task<ActionResult> ActualizarSeguridadUsuario(int id, [FromBody] UpdateUsuarioSeguridadDto dto)
        {
            if (dto is null || dto.UsuarioId <= 0)
            {
                return BadRequest("Datos de seguridad inválidos.");
            }

            UsuarioSeguridad? seguridad = await this.repo.GetSeguridadUsuario(dto.UsuarioId);
            if (seguridad is null)
            {
                return NotFound("Seguridad de usuario no encontrada.");
            }

            if (seguridad.Id != id)
            {
                return BadRequest("El id de seguridad no coincide con el usuario indicado.");
            }

            seguridad.UsuarioId = dto.UsuarioId;
            seguridad.PasswordHash = Convert.FromBase64String(dto.PasswordHash);
            seguridad.Salt = dto.Salt;
            seguridad.Activo = dto.Activo;

            await this.repo.ActualizarSeguridadUsuarioAsync(seguridad);
            return NoContent();
        }
    }
}
