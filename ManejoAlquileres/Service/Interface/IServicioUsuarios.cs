using ManejoAlquileres.Models;

namespace ManejoAlquileres.Service.Interface
{
    public interface IServicioUsuarios
    {
        Task Crear(Usuario usuario);
        Task<List<Usuario>> ObtenerTodos();
        Task<Usuario> ObtenerPorId(string id);
        Task Actualizar(Usuario usuario);
        Task Borrar(string id);
        Task<bool> Existe(string email, string id = null);
        Task<string> ObtenerUsuarioId();
    }
}
