using System.Security.Claims;
using Dapper;
using ManejoAlquileres.Models;
using ManejoAlquileres.Models.Helpers;
using ManejoAlquileres.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ManejoAlquileres.Service
{
    public class ServicioUsuarios : IServicioUsuarios
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ServicioUsuarios(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Crear(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Usuario>> ObtenerTodos()
        {
            return await _context.Usuarios.ToListAsync();
        }

        public async Task<Usuario> ObtenerPorId(string id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task Actualizar(Usuario usuario)
        {
            var original = await _context.Usuarios.FindAsync(usuario.Id_usuario);
            if (original == null) throw new Exception("Usuario no encontrado");

            original.Nombre = usuario.Nombre;
            original.Apellidos = usuario.Apellidos;
            original.Contraseña = usuario.Contraseña;
            original.NIF = usuario.NIF;
            original.Direccion = usuario.Direccion;
            original.Telefono = usuario.Telefono;
            original.Email = usuario.Email;
            original.Informacion_bancaria = usuario.Informacion_bancaria;
            original.EsAdministrador = usuario.EsAdministrador;
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task Borrar(string id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> Existe(string email, string id = null)
        {
            if (id == null)
            {
                return await _context.Usuarios.AnyAsync(u => u.Email == email);
            }
            else
            {
                return await _context.Usuarios.AnyAsync(u => u.Email == email && u.Id_usuario != id);
            }
        }

        public Task<string> ObtenerUsuarioId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Task.FromResult(userId);
        }
        public async Task<List<Usuario>> ObtenerUsuarios()
        {
            return await _context.Usuarios.ToListAsync();
        }

        public async Task<List<Usuario>> ObtenerPorListaIds(List<string> ids)
        {
            return await _context.Usuarios
                .Where(u => ids.Contains(u.Id_usuario))
                .ToListAsync();
        }
        public async Task<List<UsuarioViewModel>> ObtenerUsuariosConNombreCompleto()
        {
            return await _context.Usuarios
                .Select(u => new UsuarioViewModel
                {
                    Id_usuario = u.Id_usuario,
                    NIF = u.NIF,
                    NombreCompleto = $"{u.NIF} - {u.Nombre} {u.Apellidos}"
                })
                .ToListAsync();
        }

    }
}