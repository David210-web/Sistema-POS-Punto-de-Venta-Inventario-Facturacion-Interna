namespace SistemaFacturacionPOS.DTOs
{
    public class AjusteStockRequest
    {
        public int Cantidad { get; set; }
        public string Justificacion { get; set; } = string.Empty;
    }
}
