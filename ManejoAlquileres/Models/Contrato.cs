using System.ComponentModel.DataAnnotations;

namespace ManejoAlquileres.Models
{
    public class Contrato
    {
        [Key]
        [Required]
        [StringLength(9)]
        public string Id_contrato { get; set; }
        [Required]
        [StringLength(9)]
        public string Id_propiedad { get; set; }
        public Propiedad Propiedad { get; set; }

        [StringLength(9)]
        public string Id_habitacion { get; set; }
        public Habitacion Habitacion { get; set; }
        public DateTime Fecha_inicio { get; set; }
        public DateTime Fecha_fin { get; set; }

        [StringLength(50)]
        public string Tipo_Alquiler { get; set; }

        public decimal Porcentaje_incremento { get; set; }

        [StringLength(300)]
        public string Clausula_prorroga { get; set; }

        public DateTime Fecha_max_revision { get; set; }

        public decimal Fianza { get; set; }

        public decimal Comision_inmobiliaria { get; set; }

        [StringLength(300)]
        public string Aval { get; set; }

        [Required]
        [StringLength(50)]
        public string Periodicidad { get; set; }

        public List<Pago> Pagos { get; set; }
        public List<ContratoInquilino> Inquilinos { get; set; }
        public List<ContratoPropietario> Propietarios { get; set; }
    }
}