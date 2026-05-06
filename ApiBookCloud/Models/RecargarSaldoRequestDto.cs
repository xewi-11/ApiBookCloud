namespace ApiBookCloud.Models
{
    public class RecargarSaldoRequestDto
    {
        public int UsuarioId { get; set; }
        public decimal Monto { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}