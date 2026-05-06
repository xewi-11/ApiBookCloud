namespace ApiBookCloud.Models
{
    public class PedidoDetalleRequestDto
    {
        public int LibroId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}