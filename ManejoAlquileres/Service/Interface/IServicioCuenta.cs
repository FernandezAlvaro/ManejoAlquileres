using ManejoAlquileres.Models;

namespace ManejoAlquileres.Service.Interface
{
    public interface IServicioCuenta
    {
        Task<bool> RegisterAsync(Usuario usuario);
        Task<Usuario> LoginAsync(string email, string contrasena);
        Task<(bool encontrado, string email, string contrasena)> ForgotPasswordAsync(string dni);
        Task<string> GenerarNuevoIdUsuarioAsync();
    }
}
