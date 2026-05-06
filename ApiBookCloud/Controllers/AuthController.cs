using ApiBookCloud.Models;
using ApiBookCloud.Repositories;
using ApiOAuthEmpleados.Helpers;
using BookCloud.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NugetModelsBookCloud.Models;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace ApiBookCloud.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RepositoryUsuarios repo;
        private readonly HelperActionOAuthService helper;
        private readonly IHostEnvironment environment;
        private static readonly ConcurrentDictionary<string, (string Code, DateTime ExpiresUtc)> ResetCodes = new();

        public AuthController(RepositoryUsuarios repo, HelperActionOAuthService helper, IHostEnvironment environment)
        {
            this.repo = repo;
            this.helper = helper;
            this.environment = environment;
        }


        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> Register(RegisterRequestModel model)
        {
            string email = model.Correo;
            string password = model.Pass;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest("Correo y password son obligatorios.");
            }

            if (password.Length > 50)
            {
                return BadRequest("La contraseña no puede tener más de 50 caracteres.");
            }

            Usuario? existingUser = await this.repo.GetUserByEmail(email);
            if (existingUser is not null)
            {
                return Conflict("Ya existe un usuario con ese correo.");
            }

            Usuario user = new Usuario
            {
                Nombre = model.Nombre,
                Correo = email,
                Password = password,
                FechaRegistro = DateTime.UtcNow,
                Activo = true
            };

            string salt = EncryptionPassword.GenerateSalt();
            byte[] passwordHash = EncryptionPassword.EncryptPassword(password, salt);

            UsuarioSeguridad seguridad = new UsuarioSeguridad
            {
                PasswordHash = passwordHash,
                Salt = salt,
                Activo = true
            };

            await this.repo.CreateUserASync(user, seguridad);

            return NoContent();
        }


        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> Login(LoginRequestModel model)
        {
            string email = model.Email;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(model.Password))
            {
                return BadRequest("Correo y password son obligatorios.");
            }

            Usuario? user = await this.repo.GetUserByEmail(email);
            if (user is null)
            {
                return Unauthorized();
            }

            UsuarioSeguridad? seguridad = await this.repo.GetSeguridadUsuario(user.Id);
            if (seguridad is null)
            {
                return Unauthorized();
            }

            byte[] inputHash = EncryptionPassword.EncryptPassword(model.Password, seguridad.Salt);
            if (!EncryptionPassword.CompareArrays(seguridad.PasswordHash, inputHash))
            {
                return Unauthorized();
            }

            string token = this.CreateToken(user);
            return Ok(token);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Correo))
            {
                return BadRequest("Debes indicar un correo.");
            }

            Usuario? user = await this.repo.GetUserByEmail(request.Correo);
            if (user is null)
            {
                return Ok(new { message = "Si el correo existe, se ha generado el código de recuperación." });
            }

            string code = Random.Shared.Next(100000, 999999).ToString();
            ResetCodes[request.Correo.ToLowerInvariant()] = (code, DateTime.UtcNow.AddMinutes(15));

            if (this.environment.IsDevelopment())
            {
                return Ok(new
                {
                    message = "Código generado en entorno de desarrollo.",
                    code
                });
            }

            return Ok(new { message = "Si el correo existe, se ha generado el código de recuperación." });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Correo) || string.IsNullOrWhiteSpace(request.Codigo) || string.IsNullOrWhiteSpace(request.NuevaPassword))
            {
                return BadRequest("Debes indicar correo, código y nueva contraseña.");
            }

            string emailKey = request.Correo.ToLowerInvariant();
            if (!ResetCodes.TryGetValue(emailKey, out var resetInfo))
            {
                return BadRequest("No existe un código de recuperación activo para ese correo.");
            }

            if (resetInfo.ExpiresUtc < DateTime.UtcNow)
            {
                ResetCodes.TryRemove(emailKey, out _);
                return BadRequest("El código de recuperación ha expirado.");
            }

            if (!string.Equals(resetInfo.Code, request.Codigo, StringComparison.Ordinal))
            {
                return BadRequest("Código de recuperación inválido.");
            }

            Usuario? user = await this.repo.GetUserByEmail(request.Correo);
            if (user is null)
            {
                return NotFound("Usuario no encontrado.");
            }

            UsuarioSeguridad? seguridad = await this.repo.GetSeguridadUsuario(user.Id);
            if (seguridad is null)
            {
                return NotFound("Seguridad de usuario no encontrada.");
            }

            string salt = EncryptionPassword.GenerateSalt();
            byte[] hash = EncryptionPassword.EncryptPassword(request.NuevaPassword, salt);

            seguridad.Salt = salt;
            seguridad.PasswordHash = hash;
            user.Password = request.NuevaPassword;

            await this.repo.ActualizarSeguridadUsuarioAsync(seguridad);
            await this.repo.ActualizarUsuarioAsync(user);

            ResetCodes.TryRemove(emailKey, out _);
            return NoContent();
        }

        private string CreateToken(Usuario user)
        {
            SigningCredentials credentials = new(this.helper.GetKeyToken(), SecurityAlgorithms.HmacSha256);

            string userJson = JsonSerializer.Serialize(new
            {
                userId = user.Id,
                correo = user.Correo,
                nombre = user.Nombre
            });
            string userDataCifrada = HelperCryptography.CifrarString(userJson);

            List<Claim> claims = new()
            {
                new Claim("UserData", userDataCifrada)
            };

            JwtSecurityToken token = new(
                claims: claims,
                issuer: this.helper.Issuer,
                audience: this.helper.Audience,
                signingCredentials: credentials,
                expires: DateTime.UtcNow.AddMinutes(60),
                notBefore: DateTime.UtcNow
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
