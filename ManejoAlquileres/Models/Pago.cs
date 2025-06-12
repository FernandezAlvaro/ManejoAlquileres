using System.ComponentModel.DataAnnotations;

namespace ManejoAlquileres.Models
{
    public class Pago
    {
        [Key]
        [Required]
        [StringLength(9)]
        public string Id_pago { get; set; }

        [Required]
        [StringLength(9)]
        public string Id_contrato { get; set; }
        public Contrato Contrato { get; set; }
        [Required]
        public DateTime Fecha_pago_programada { get; set; }

        public DateTime? Fecha_pago_real { get; set; }

        [Required]
        public decimal Monto_pago { get; set; }

        [StringLength(300)]
        public string? Descripcion { get; set; }

        [StringLength(300)]
        public string? Archivo_factura { get; set; }
    }
}
