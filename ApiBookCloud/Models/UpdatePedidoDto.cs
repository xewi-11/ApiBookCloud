namespace ApiBookCloud.Models
{
    public class UpdatePedidoDto
    {
        public int UsuarioId { get; set; }
        public DateTime FechaPedido { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
