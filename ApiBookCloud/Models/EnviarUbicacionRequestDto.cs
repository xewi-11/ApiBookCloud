namespace ApiBookCloud.Models
{
    public class EnviarUbicacionRequestDto
    {
        public int RemitenteId { get; set; }
        public decimal Latitud { get; set; }
        public decimal Longitud { get; set; }
    }
}