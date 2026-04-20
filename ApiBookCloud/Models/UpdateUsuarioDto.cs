namespace ApiBookCloud.Models
{
    public class UpdateUsuarioDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string? Password { get; set; }
        public bool Activo { get; set; }
        public string? Foto { get; set; }
    }
}
