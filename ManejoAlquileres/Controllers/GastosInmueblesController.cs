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

        public async Task<IActionResult> Index()
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

            ViewBag.PropiedadId = propiedadesDict;
            return View(gastos);
        }

        public async Task<IActionResult> Crear(string propiedadId)
        {
            if (!await UsuarioEsPropietario(propiedadId))
                return Forbid();

            return View(new GastoInmueble { Id_propiedad = propiedadId });
        }

        [HttpPost]
        public async Task<IActionResult> Crear(GastoInmueble gasto)
        {
            if (!await UsuarioEsPropietario(gasto.Id_propiedad))
                return Forbid();

            if (!ModelState.IsValid)
                return View(gasto);

            await _servicio.Crear(gasto);
            return RedirectToAction("Index", new { propiedadId = gasto.Id_propiedad });
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
            return RedirectToAction("Index", new { propiedadId = gasto.Id_propiedad });
        }

    }
}