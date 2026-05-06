namespace ApiBookCloud.Models
{
    public class StripeCheckoutCreateRequestDto
    {
        public string TipoOperacion { get; set; } = string.Empty;
        public decimal? MontoRecarga { get; set; }
        public decimal? Total { get; set; }
        public List<PedidoDetalleRequestDto> Detalles { get; set; } = new();
    }
}