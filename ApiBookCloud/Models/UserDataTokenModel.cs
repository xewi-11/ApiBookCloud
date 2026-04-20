namespace ApiBookCloud.Models
{
    public class UserDataTokenModel
    {
        public int UserId { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }
}
