using ApiBookCloud.Models;
using ApiBookCloud.Repositories;
using ApiOAuthEmpleados.Helpers;
using BookCloud.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NugetModelsBookCloud.Models;
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

        public AuthController(RepositoryUsuarios repo, HelperActionOAuthService helper)
        {
            this.repo = repo;
            this.helper = helper;
        }


        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> Register(RegisterModel model)
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
        public async Task<ActionResult> Login(LoginModel model)
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
