namespace ApiBookCloud.Models
{
    public class RegisterRequestModel
    {
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Pass { get; set; } = string.Empty;
    }
}
