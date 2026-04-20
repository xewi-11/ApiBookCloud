namespace ApiBookCloud.Models
{
    public class UpdateSaldoMovimientoDto
    {
        public int UsuarioId { get; set; }
        public int? PedidoId { get; set; }
        public decimal Monto { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public bool Activo { get; set; }
    }
}
