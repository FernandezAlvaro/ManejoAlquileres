namespace ManejoAlquileres.Models.DTO
{
    public class PagoConContratoDTO
    {
        public string Id_pago { get; set; }
        public DateTime Fecha_pago_programada { get; set; }
        public DateTime? Fecha_pago_real { get; set; }
        public decimal Monto_pago { get; set; }
        public string? Descripcion { get; set; }
        public string? Archivo_factura { get; set; }

        public string Id_contrato { get; set; }
        public string Direccion_propiedad { get; set; }
        public DateTime Fecha_inicio_contrato { get; set; }
        public DateTime Fecha_fin_contrato { get; set; }

        public List<string> Id_inquilinos { get; set; }
        public List<string> Id_duenios { get; set; }
    }
}
