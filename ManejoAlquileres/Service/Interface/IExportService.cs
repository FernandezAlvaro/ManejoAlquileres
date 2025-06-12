using ManejoAlquileres.Models.Helpers;

namespace ManejoAlquileres.Service.Interface
{
    public interface IExportService
    {
        ArchivoExportado ExportarPagosFiltrados(DateTime? desde, DateTime? hasta, string estado, string tipo, string formato, string userId);
        ArchivoExportado ExportarPropiedadesUsuario(string userId, string formato);
        ArchivoExportado ExportarTodaBaseDeDatos(string formato);
    }
}
