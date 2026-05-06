namespace ApiBookCloud.Models
{
    public class EnviarMensajeRequestDto
    {
        public int RemitenteId { get; set; }
        public string Contenido { get; set; } = string.Empty;
    }
}