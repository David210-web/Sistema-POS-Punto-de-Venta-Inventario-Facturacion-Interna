namespace SistemaFacturacionPOS.DTOs
{
    public class ExistenciaRequest
    {
        public Guid ProductoId { get; set; }
        public Guid BodegaId { get; set; }
        public int Stock { get; set; }
    }
}
