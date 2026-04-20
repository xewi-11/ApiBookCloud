using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ApiOAuthEmpleados.Helpers
{
    public class HelperActionOAuthService
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SecretKey { get; set; }

        public HelperActionOAuthService(IConfiguration configuration)
        {
            this.Issuer = configuration.GetValue<string>
                ("ApiOAuthToken:Issuer");
            this.Audience = configuration.GetValue<string>
                ("ApiOAuthToken:Audience");
            this.SecretKey = configuration.GetValue<string>
                ("ApiOAuthToken:SecretKey");
        }

        //NECESITAMOS UN METODO PARA GENERAR EL TOKEN A PARTIR
        //DE NUESTRO SECRET KEY
        public SymmetricSecurityKey GetKeyToken()
        {
            //CONVERTIMOS A BYTES NUESTRO SECRET KEY
            byte[] data =
                Encoding.UTF8.GetBytes(this.SecretKey);
            return new SymmetricSecurityKey(data);
        }

        //UTILIZAMOS CLASES ACTION PARA SEPARAR LA CAPA 
        //DE LOS SERVICES DE AUTORIZACION DEL PROGRAM
        public Action<JwtBearerOptions> GetJWtBearerOptions()
        {
            Action<JwtBearerOptions> options =
                new Action<JwtBearerOptions>(options =>
                {
                    //INDICAMOS LO QUE SE VA A VALIDAR DENTRO DEL 
                    //TOKEN PARA PERMITIR EL ACCESO
                    options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = this.Issuer,
                        ValidAudience = this.Audience,
                        IssuerSigningKey = this.GetKeyToken()
                    };
                });
            return options;
        }

        //EL ESQUEMA DE NUESTRA VALIDACION JwtBearerDefaults
        public Action<AuthenticationOptions> GetAuthenticationSchema()
        {
            Action<AuthenticationOptions> options =
                new Action<AuthenticationOptions>(options =>
                {
                    options.DefaultScheme =
                    JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;
                });
            return options;
        }
    }
}
