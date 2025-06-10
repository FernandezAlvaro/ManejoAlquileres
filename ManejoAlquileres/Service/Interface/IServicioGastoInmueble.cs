using ManejoAlquileres.Models;

namespace ManejoAlquileres.Service.Interface
{
    public interface IServicioGastoInmueble
    {
        Task Crear(GastoInmueble gasto);
        Task<GastoInmueble> ObtenerPorId(string id);
        Task<GastoInmueble> ObtenerConPropiedad(string id);
        Task Actualizar(GastoInmueble gasto);
        Task Borrar(string id);
        Task<bool> Existe(string id);
        Task<List<GastoInmueble>> ObtenerPorPropiedades(List<string> propiedadIds);
        Task<List<GastoInmueble>> ObtenerTodos();
    }
}
