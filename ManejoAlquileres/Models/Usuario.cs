using System.ComponentModel.DataAnnotations;

namespace ManejoAlquileres.Models
{
    public class Usuario
    {
        [Key]
        [StringLength(9)]
        public string Id_usuario { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(50)]
        public string Apellidos { get; set; }
        [Required]
        [StringLength(25)]
        public string Contraseña { get; set; }

        [Required]
        [StringLength(9)]
        public string NIF { get; set; }

        [Required]
        [StringLength(300)]
        public string Direccion { get; set; }

        [Required]
        [StringLength(9)]
        public string Telefono { get; set; }

        [Required]
        [StringLength(150)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public bool EsAdministrador { get; set; }

        [Required]
        [StringLength(20)]
        public string Informacion_bancaria { get; set; }

        public List<ContratoInquilino> ContratosComoInquilino { get; set; }
        public List<ContratoPropietario> ContratosComoPropietario { get; set; }
        public List<PropiedadUsuario> Propiedades { get; set; }
    }
}
