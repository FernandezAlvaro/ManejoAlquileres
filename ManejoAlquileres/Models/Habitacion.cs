using System.ComponentModel.DataAnnotations;

namespace ManejoAlquileres.Models
{
    public class Habitacion
    {
        [Key]
        [Required]
        [StringLength(9)]
        public string Id_habitacion { get; set; }

        [Required]
        [StringLength(9)]
        public string Id_propiedad { get; set; }
        public Propiedad Propiedad { get; set; }
        public decimal Tamaño { get; set; }

        [StringLength(300)]
        public string Descripcion { get; set; }

        [Required]
        public bool Disponible { get; set; }
        [Required]
        public bool Bano_propio { get; set; }

        public List<Contrato> Contratos { get; set; }
    }
}
