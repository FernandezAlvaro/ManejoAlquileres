namespace ManejoAlquileres.Models
{
    public class ContratoPropietario
    {
        public string ContratoId { get; set; }
        public Contrato Contrato { get; set; }

        public string UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
    }
}
