using System.ComponentModel.DataAnnotations;

namespace ManejoAlquileres.Models
{
    public class GastoInmueble
    {
        [Key]
        [Required]
        [StringLength(9)]
        public string Id_gasto { get; set; }

        [Required]
        [StringLength(9)]
        public string Id_propiedad { get; set; }
        public Propiedad Propiedad { get; set; }

        [Required]
        [StringLength(50)]
        public string Tipo_gasto { get; set; }

        [Required]
        public decimal Monto_gasto { get; set; }

        public DateTime Fecha_pago { get; set; }

        public decimal Porcentaje_amortizacion { get; set; }

        [Required]
        public bool Repercutible { get; set; }

        [StringLength(300)]
        public string Descripcion { get; set; }
    }
}
