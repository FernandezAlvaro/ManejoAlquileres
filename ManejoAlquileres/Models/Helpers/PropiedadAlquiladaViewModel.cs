namespace ManejoAlquileres.Models.Helpers
{
    public class PropiedadAlquiladaViewModel
    {
        public string PropiedadId { get; set; }
        public string Direccion { get; set; }
        public string TipoAlquiler { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Periodicidad { get; set; }
        public decimal Fianza { get; set; }
        public bool EsHabitacion { get; set; }

        public List<Habitacion> Habitaciones { get; set; }
        public List<Contrato> Contratos { get; set; }
        public List<GastoInmueble> GastosInmueble { get; set; }
    }
}
