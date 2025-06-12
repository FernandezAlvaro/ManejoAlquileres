using System.ComponentModel.DataAnnotations;

namespace ManejoAlquileres.Models.Helpers
{
    public class ContratoViewModel
    {
        public string Id_contrato { get; set; }
        public DateTime Fecha_inicio { get; set; }
        public DateTime Fecha_fin { get; set; }
        public DateTime Fecha_max_revision { get; set; }
        [Required(ErrorMessage = "Debe seleccionar el tipo de alquiler")]
        public string Tipo_Alquiler { get; set; }
        public decimal Monto_pago { get; set; }
        public decimal Porcentaje_incremento { get; set; }
        public decimal Fianza { get; set; }
        public string Clausula_prorroga { get; set; }
        public decimal Comision_inmobiliaria { get; set; }
        public string Periodicidad { get; set; }
        public string Aval { get; set; }

        public List<string> InquilinosSeleccionados { get; set; }

        public Propiedad Propiedad { get; set; }
        public Habitacion? Habitacion { get; set; }
        public Usuario Inquilino { get; set; }

        public List<Usuario> Inquilinos { get; set; }
        public List<Usuario> Propietarios { get; set; }
        public List<Pago> Pagos { get; set; }

        public List<ContratoInquilino> ContratosInquilinos { get; set; }
        public List<ContratoPropietario> ContratosPropietarios { get; set; }
        public List<PropiedadUsuario> PropiedadesUsuarios { get; set; }
    }
}
