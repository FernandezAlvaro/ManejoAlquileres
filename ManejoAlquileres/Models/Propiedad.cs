using System.ComponentModel.DataAnnotations;

namespace ManejoAlquileres.Models
{
    public class Propiedad
    {
        [Key]
        [Required]
        [StringLength(9)]
        public string Id_propiedad { get; set; }

        [Required]
        [StringLength(150)]
        public string Direccion { get; set; }

        [Required]
        [StringLength(20)]
        public string Referencia_catastral { get; set; }
        [Required]
        public int numHabitaciones { get; set; }

        public decimal Valor_catastral_piso { get; set; }

        public decimal Valor_catastral_terreno { get; set; }

        public DateTime Fecha_adquisicion { get; set; }

        public decimal Valor_adquisicion { get; set; }

        public decimal Valor_adqui_total { get; set; }
        [Required]
        public bool Estado_propiedad { get; set; }

        [StringLength(300)]
        public string Descripcion { get; set; }


        public List<PropiedadUsuario> Usuarios { get; set; } = new();
        public List<Habitacion> Habitaciones { get; set; } = new();
        public List<Contrato> Contratos { get; set; } = new();
        public List<GastoInmueble> GastoInmueble { get; set; } = new();
    }
}
