using ManejoAlquileres.Models;
using ManejoAlquileres.Service.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManejoAlquileres.Controllers
{
    [Authorize(Policy = "EsAdministrador")]
    public class UsuariosController : Controller
    {
        private readonly IServicioUsuarios _servicioUsuarios;
        private readonly IGeneradorIdsService _generadorIdsService;

        public UsuariosController(IServicioUsuarios servicioUsuarios, IGeneradorIdsService generadorIdsService)
        {
            _servicioUsuarios = servicioUsuarios;
            _generadorIdsService = generadorIdsService;
        }

        public async Task<IActionResult> Index()
        {
            var usuarios = await _servicioUsuarios.ObtenerTodos();
            return View(usuarios);
        }

        public async Task<IActionResult> Details(string id)
        {

            if (id == null)
                return NotFound();

            var usuario = await _servicioUsuarios.ObtenerPorId(id);
            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        public IActionResult Create()
        {
            ViewBag.Modo = "Create";
            return View("UsuarioForm", new Usuario());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Usuario model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Modo = "Create";
                return View("UsuarioForm", model);
            }

            var nuevoId = await _generadorIdsService.GenerarIdUnicoAsync();
            model.Id_usuario = nuevoId;
            await _servicioUsuarios.Crear(model);

            TempData["Mensaje"] = "Usuario creado correctamente.";
            TempData["TipoMensaje"] = "success";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
                return NotFound();

            var usuario = await _servicioUsuarios.ObtenerPorId(id);
            if (usuario == null)
                return NotFound();

            ViewBag.Modo = "Edit";
            return View("UsuarioForm", usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Usuario usuario)
        {
            if (id != usuario.Id_usuario)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Modo = "Edit";
                return View("UsuarioForm", usuario);
            }

            // Obtener la instancia original que ya está siendo trackeada
            var usuarioOriginal = await _servicioUsuarios.ObtenerPorId(id);
            if (usuarioOriginal == null)
                return NotFound();

            var idUsuarioActual = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            bool cambioRolAdmin = usuarioOriginal.EsAdministrador != usuario.EsAdministrador;
            bool esElMismoUsuario = idUsuarioActual == usuario.Id_usuario;

            // Actualizar los campos de la instancia original
            usuarioOriginal.Nombre = usuario.Nombre;
            usuarioOriginal.Apellidos = usuario.Apellidos;
            usuarioOriginal.Contraseña = usuario.Contraseña;
            usuarioOriginal.NIF = usuario.NIF;
            usuarioOriginal.Direccion = usuario.Direccion;
            usuarioOriginal.Telefono = usuario.Telefono;
            usuarioOriginal.Email = usuario.Email;
            usuarioOriginal.Informacion_bancaria = usuario.Informacion_bancaria;
            usuarioOriginal.EsAdministrador = usuario.EsAdministrador;

            // Guardar cambios solo sobre la instancia original
            await _servicioUsuarios.Actualizar(usuarioOriginal);

            if (cambioRolAdmin && esElMismoUsuario)
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction("Login", "Cuenta");
            }

            TempData["Mensaje"] = "Usuario actualizado correctamente.";
            TempData["TipoMensaje"] = "success";
            return RedirectToAction(nameof(Index));
        }




        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
                return NotFound();

            var usuario = await _servicioUsuarios.ObtenerPorId(id);
            if (usuario == null)
            {
                return NotFound();
            }

            ViewBag.Modo = "Eliminar";
            return View("UsuarioForm", usuario);
        }


        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _servicioUsuarios.Borrar(id);

            TempData["Mensaje"] = "Usuario eliminado correctamente.";
            TempData["TipoMensaje"] = "danger";
            return RedirectToAction(nameof(Index));
        }

    }
}