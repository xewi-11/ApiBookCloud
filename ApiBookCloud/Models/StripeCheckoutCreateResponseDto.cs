namespace ApiBookCloud.Models
{
    public class StripeCheckoutCreateResponseDto
    {
        public string SessionId { get; set; } = string.Empty;
        public string CheckoutUrl { get; set; } = string.Empty;
        public int? PedidoId { get; set; }
    }
}