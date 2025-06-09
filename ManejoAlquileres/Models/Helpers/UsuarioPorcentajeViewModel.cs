namespace ManejoAlquileres.Models.Helpers
{
    public class UsuarioPorcentajeViewModel
    {
        public string UsuarioId { get; set; }  // Para la edición/guardado
        public string NIF { get; set; }        // DNI del usuario
        public string NombreCompleto { get; set; }  // Nombre + Apellidos para mostrar
        public decimal Porcentaje { get; set; }
    }
}

