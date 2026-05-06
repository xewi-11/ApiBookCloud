namespace ApiBookCloud.Models
{
    public class CrearPagoRequestDto
    {
        public int PedidoId { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal Monto { get; set; }
        public string Metodo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
    }
}