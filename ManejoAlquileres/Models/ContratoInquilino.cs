namespace ManejoAlquileres.Models
{
    public class ContratoInquilino
    {
        public string ContratoId { get; set; }
        public Contrato Contrato { get; set; }

        public string UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
    }
}
