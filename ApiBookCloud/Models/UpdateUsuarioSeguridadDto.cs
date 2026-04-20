namespace ApiBookCloud.Models
{
    public class UpdateUsuarioSeguridadDto
    {
        public int UsuarioId { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
