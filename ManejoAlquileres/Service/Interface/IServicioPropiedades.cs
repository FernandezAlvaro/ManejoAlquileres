using ManejoAlquileres.Models;

namespace ManejoAlquileres.Service.Interface
{
    public interface IServicioPropiedades
    {
        Task Crear(Propiedad propiedad);
        Task<Propiedad> ObtenerPorId(string id);
        Task<List<Propiedad>> ObtenerTodas();
        Task Actualizar(Propiedad propiedad);
        Task Borrar(string id);
        Task<bool> ExisteReferenciaCatastral(string referenciaCatastral, string id = null);
        Task<List<Propiedad>> ObtenerPropiedadesComoPropietarioAsync();
        Task<List<Propiedad>> ObtenerPropiedadesComoInquilinoAsync();

    }
}
