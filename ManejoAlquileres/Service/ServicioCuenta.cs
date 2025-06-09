using ManejoAlquileres;
using ManejoAlquileres.Models;
using ManejoAlquileres.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

public class ServicioCuenta : IServicioCuenta
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IGeneradorIdsService _generadorIdsService;

    public ServicioCuenta(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IGeneradorIdsService generadorIdsService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _generadorIdsService = generadorIdsService;
    }

    public async Task<bool> RegisterAsync(Usuario model)
    {
        if (string.IsNullOrEmpty(model.Id_usuario))
        {
            model.Id_usuario = await GenerarNuevoIdUsuarioAsync();
        }

        var existe = await _context.Usuarios
            .AnyAsync(u => u.Id_usuario == model.Id_usuario || u.NIF == model.NIF);

        if (existe)
            return false;

        _context.Usuarios.Add(model);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Usuario> LoginAsync(string email, string contrasena)
    {
        var usuario = await _context.Usuarios
            .Where(u => u.Email == email && u.Contraseña == contrasena)
            .Select(u => new Usuario
            {
                Id_usuario = u.Id_usuario,
                Nombre = u.Nombre,
                Apellidos = u.Apellidos,
                Email = u.Email,
                EsAdministrador = u.EsAdministrador,
                Contraseña = u.Contraseña
            })
            .FirstOrDefaultAsync();

        return usuario;
    }

    public async Task<(bool encontrado, string email, string contrasena)> ForgotPasswordAsync(string dni)
    {
        var usuario = await _context.Usuarios
            .Where(u => u.NIF == dni)
            .Select(u => new { u.Email, u.Contraseña })
            .FirstOrDefaultAsync();

        if (usuario == null)
            return (false, null, null);

        return (true, usuario.Email, usuario.Contraseña);
    }

    public Task<string> ObtenerUsuarioId()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Task.FromResult(userId);
    }

    public async Task<string> GenerarNuevoIdUsuarioAsync()
    {
        var generador = await _generadorIdsService.GenerarIdUnicoAsync();
        return generador;
    }

    public async Task<bool> ExisteIdAsync(string idUsuario)
    {
        return await _context.Usuarios.AnyAsync(u => u.Id_usuario == idUsuario);
    }
}