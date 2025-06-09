using ManejoAlquileres.Models;
using ManejoAlquileres.Service.Interface;
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

            await _servicioUsuarios.Actualizar(usuario);

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