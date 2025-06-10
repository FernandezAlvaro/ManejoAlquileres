using System.Security.Claims;
using ManejoAlquileres.Models;
using ManejoAlquileres.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManejoAlquileres.Controllers
{
    [Authorize(Policy = "UsuarioAutenticado")]
    public class GastosInmueblesController : Controller
    {
        private readonly IServicioGastoInmueble _servicio;
        private readonly ApplicationDbContext _context;

        public GastosInmueblesController(IServicioGastoInmueble servicio, ApplicationDbContext context)
        {
            _servicio = servicio;
            _context = context;
        }

        private async Task<bool> UsuarioEsPropietario(string propiedadId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _context.PropiedadesUsuarios
                .AnyAsync(pu => pu.UsuarioId == userId && pu.PropiedadId == propiedadId);
        }

        public async Task<IActionResult> Index(string? propiedadId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var propiedades = await _context.PropiedadesUsuarios
                .Include(pu => pu.Propiedad)
                .Where(pu => pu.UsuarioId == userId)
                .Select(pu => pu.Propiedad)
                .ToListAsync();

            var propiedadIds = propiedades.Select(p => p.Id_propiedad).ToList();

            var gastos = await _servicio.ObtenerPorPropiedades(propiedadIds);

            var propiedadesDict = propiedades.ToDictionary(p => p.Id_propiedad);
            ViewBag.PropiedadesReferencias = propiedadesDict.ToDictionary(p => p.Key, p => p.Value.Referencia_catastral);

            if (!string.IsNullOrEmpty(propiedadId))
            {
                gastos = gastos.Where(g => g.Id_propiedad == propiedadId).ToList();
                ViewBag.PropiedadId = propiedadId;
                ViewBag.NombrePropiedad = propiedadesDict.ContainsKey(propiedadId) ? propiedadesDict[propiedadId].Referencia_catastral : "";
            }
            else
            {
                ViewBag.PropiedadId = null;
                ViewBag.NombrePropiedad = "todas las propiedades";
            }

            return View(gastos);
        }


        public async Task<IActionResult> Editar(string id)
        {
            var gasto = await _servicio.ObtenerPorId(id);
            if (gasto == null || !await UsuarioEsPropietario(gasto.Id_propiedad))
                return Forbid();

            return View(gasto);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(GastoInmueble gasto)
        {
            if (!await UsuarioEsPropietario(gasto.Id_propiedad))
                return Forbid();

            if (!ModelState.IsValid)
                return View(gasto);
            TempData["Mensaje"] = "Gasto editado correctamente.";
            await _servicio.Actualizar(gasto);
            return RedirectToAction("Index", new { propiedadId = gasto.Id_propiedad });
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(string id)
        {
            var gasto = await _servicio.ObtenerPorId(id);
            if (gasto == null || !await UsuarioEsPropietario(gasto.Id_propiedad))
                return Forbid();

            await _servicio.Borrar(id);
            TempData["Mensaje"] = "Gasto eliminado correctamente.";
            return RedirectToAction("Index", new { propiedadId = gasto.Id_propiedad });
        }

        public async Task<IActionResult> Detalles(string id)
        {
            var gasto = await _servicio.ObtenerConPropiedad(id);
            if (gasto == null || !await UsuarioEsPropietario(gasto.Id_propiedad))
                return Forbid();

            return View(gasto);
        }
        public async Task<IActionResult> ListaCompletaGastos()
        {
            var gastos = await _servicio.ObtenerTodos();
            return View(gastos);
        }
        public async Task<IActionResult> ModificarAdmin(string id)
        {
            var gasto = await _servicio.ObtenerPorId(id);
            if (gasto == null)
                return NotFound();

            return View(gasto);
        }

        [HttpPost]
        public async Task<IActionResult> ModificarAdmin(GastoInmueble gasto)
        {
            if (!ModelState.IsValid)
                return View(gasto);

            await _servicio.Actualizar(gasto);
            TempData["Mensaje"] = "Gasto modificado correctamente.";
            return RedirectToAction("ListaCompletaGastos");
        }

        public async Task<IActionResult> DetallesAdmin(string id)
        {
            var gasto = await _servicio.ObtenerConPropiedad(id);
            if (gasto == null)
                return NotFound();

            return View(gasto);
        }
    }
}