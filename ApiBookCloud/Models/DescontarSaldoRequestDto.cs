namespace ApiBookCloud.Models
{
    public class DescontarSaldoRequestDto
    {
        public int UsuarioId { get; set; }
        public int PedidoId { get; set; }
        public decimal Monto { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}