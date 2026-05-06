using Microsoft.AspNetCore.Http;

namespace ApiBookCloud.Models
{
    public class CreateLibroRequestDto
    {
        public string Titulo { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public bool Activo { get; set; } = true;
        public IFormFile? Foto { get; set; }
    }
}