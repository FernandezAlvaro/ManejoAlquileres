namespace ManejoAlquileres.Models.Helpers
{
    public class PropiedadAdminViewModel
    {
        public string Id { get; set; }

        public string Direccion { get; set; }

        public string ReferenciaCatastral { get; set; }

        public int NumHabitaciones { get; set; }

        public decimal ValorCatastralPiso { get; set; }

        public decimal ValorCatastralTerreno { get; set; }

        public DateTime FechaAdquisicion { get; set; }

        public decimal ValorAdquisicion { get; set; }

        public decimal ValorAdquisicionTotal { get; set; }

        public bool EstadoPropiedad { get; set; }

        public string Descripcion { get; set; }

        public List<UsuarioPorcentajeViewModel> PorcentajesUsuarios { get; set; }

    }
}
