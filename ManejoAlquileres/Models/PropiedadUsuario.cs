using System.ComponentModel.DataAnnotations;

namespace ManejoAlquileres.Models
{
    public class PropiedadUsuario
    {
        public string UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        public string PropiedadId { get; set; }
        public Propiedad Propiedad { get; set; }

        [Required]
        public decimal PorcentajePropiedad { get; set; }
    }
}
