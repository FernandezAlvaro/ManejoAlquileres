using ManejoAlquileres.Models;
using ManejoAlquileres.Models.Helpers;

namespace ManejoAlquileres.Service.Interface
{
    public interface IServicioPropiedadUsuario
    {
        Task<bool> UsuarioYaTieneRelacion(string usuarioId, string propiedadId);
        Task<decimal> ObtenerPorcentajeTotal(string propiedadId);
        Task AgregarRelacion(string usuarioId, string propiedadId, decimal porcentaje);
        Task EliminarRelacionOPropiedadSiEsUnica(string propiedadId, string usuarioId);
        Task<PropiedadViewModel> ObtenerPropiedadConRelaciones(string propiedadId, string usuarioId);
    }
}
