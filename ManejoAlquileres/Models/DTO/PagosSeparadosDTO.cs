namespace ManejoAlquileres.Models.DTO
{
    public class PagosSeparadosDTO
    {
        public List<PagoConContratoDTO> PagosComoInquilino { get; set; }
        public List<PagoConContratoDTO> PagosComoPropietario { get; set; }
    }
}
