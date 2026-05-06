namespace ApiBookCloud.Models
{
    public class CrearPedidoRequestDto
    {
        public int UsuarioId { get; set; }
        public decimal Total { get; set; }
        public List<PedidoDetalleRequestDto> Detalles { get; set; } = new();
    }
}