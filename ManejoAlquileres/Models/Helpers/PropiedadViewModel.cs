using System.ComponentModel.DataAnnotations;

namespace ManejoAlquileres.Models.Helpers
{
    public class PropiedadViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Dirección")]
        public string Direccion { get; set; }

        [Display(Name = "Referencia Catastral")]
        public string ReferenciaCatastral { get; set; }

        [Display(Name = "Número de Habitaciones")]
        public int NumHabitaciones { get; set; }

        [Display(Name = "Valor Catastral Piso")]
        public decimal ValorCatastralPiso { get; set; }

        [Display(Name = "Valor Catastral Terreno")]
        public decimal ValorCatastralTerreno { get; set; }

        [Display(Name = "Fecha de Adquisición")]
        [DataType(DataType.Date)]
        public DateTime FechaAdquisicion { get; set; }

        [Display(Name = "Valor de Adquisición")]
        public decimal ValorAdquisicion { get; set; }

        [Display(Name = "Valor Total Adquisición")]
        public decimal ValorAdquisicionTotal { get; set; }

        [Display(Name = "Estado")]
        public bool EstadoPropiedad { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Porcentaje de Propiedad")]
        [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100")]
        public double PorcentajePropiedad { get; set; }
        public List<Habitacion> Habitaciones { get; set; }
        public List<PropiedadUsuario> Usuarios { get; set; }
        public List<Contrato> Contratos { get; set; }
        public List<GastoInmueble> GastosInmueble { get; set; }
        public List<UsuarioPorcentajeViewModel>? UsuariosConPorcentaje { get; set; }
    }
}