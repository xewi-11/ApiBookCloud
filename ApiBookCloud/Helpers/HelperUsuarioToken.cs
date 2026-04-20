using ApiBookCloud.Models;
using System.Security.Claims;
using System.Text.Json;

namespace ApiOAuthEmpleados.Helpers
{
    public class HelperUsuarioToken
    {
        private readonly IHttpContextAccessor contextAccessor;

        public HelperUsuarioToken(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        public UserDataTokenModel? GetUserData()
        {
            string? encryptedUserData = this.contextAccessor.HttpContext?.User
                .FindFirst("UserData")?.Value;

            if (!string.IsNullOrWhiteSpace(encryptedUserData))
            {
                string json = HelperCryptography.DescifrarString(encryptedUserData);
                JsonSerializerOptions options = new()
                {
                    PropertyNameCaseInsensitive = true
                };

                UserDataTokenModel? data = JsonSerializer.Deserialize<UserDataTokenModel>(json, options);
                if (data is not null && data.UserId > 0)
                {
                    return data;
                }
            }

            // Fallback por compatibilidad con tokens antiguos
            Claim? idClaim = this.contextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier);
            Claim? emailClaim = this.contextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Email);

            if (idClaim is null || !int.TryParse(idClaim.Value, out int id))
            {
                return null;
            }

            return new UserDataTokenModel
            {
                UserId = id,
                Correo = emailClaim?.Value ?? string.Empty,
                Nombre = string.Empty
            };
        }

        public int? GetUserId()
        {
            return this.GetUserData()?.UserId;
        }

        public string? GetEmail()
        {
            return this.GetUserData()?.Correo;
        }
    }
}
