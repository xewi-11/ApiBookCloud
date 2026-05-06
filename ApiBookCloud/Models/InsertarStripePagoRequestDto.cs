namespace ApiBookCloud.Models
{
    public class InsertarStripePagoRequestDto
    {
        public int UsuarioId { get; set; }
        public int? PedidoId { get; set; }
        public string StripeSessionId { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string TipoOperacion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}