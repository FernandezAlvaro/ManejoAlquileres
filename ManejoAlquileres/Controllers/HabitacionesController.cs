using System.Security.Claims;
using ManejoAlquileres.Models;
using ManejoAlquileres.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManejoAlquileres.Controllers
{
    [Authorize]
    public class HabitacionesController : Controller
    {
        private readonly IServicioHabitacion _servicio;
        private readonly ApplicationDbContext _context;

        public HabitacionesController(IServicioHabitacion servicio, ApplicationDbContext context)
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

            var habitaciones = (await _servicio.ObtenerTodas())
                .Where(h => propiedadIds.Contains(h.Id_propiedad))
                .ToList();

            var propiedadesDict = propiedades.ToDictionary(p => p.Id_propiedad);

            ViewBag.Propiedades = propiedadesDict;

            return View(habitaciones);
        }


        public async Task<IActionResult> Crear(string propiedadId)
        {
            if (!await UsuarioEsPropietario(propiedadId))
                return Forbid();

            var habitacion = new Habitacion { Id_propiedad = propiedadId };
            return View(habitacion);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Habitacion habitacion)
        {
            if (!await UsuarioEsPropietario(habitacion.Id_propiedad))
                return Forbid();

            var propiedad = await _context.Propiedades
                .Include(p => p.Habitaciones)
                .FirstOrDefaultAsync(p => p.Id_propiedad == habitacion.Id_propiedad);

            if (propiedad == null)
                return NotFound();

            if (propiedad.Habitaciones.Count >= propiedad.numHabitaciones)
            {
                ModelState.AddModelError("", $"La propiedad ya tiene el número máximo de habitaciones permitidas: {propiedad.numHabitaciones}.");
                return View(habitacion);
            }

            if (!ModelState.IsValid)
                return View(habitacion);

            await _servicio.Crear(habitacion);
            return RedirectToAction("Index", new { propiedadId = habitacion.Id_propiedad });
        }

        public async Task<IActionResult> Editar(string id)
        {
            var habitacion = await _servicio.ObtenerPorId(id);
            if (habitacion == null || !await UsuarioEsPropietario(habitacion.Id_propiedad))
                return Forbid();

            return View(habitacion);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Habitacion habitacion)
        {
            if (!await UsuarioEsPropietario(habitacion.Id_propiedad))
                return Forbid();

            if (!ModelState.IsValid)
                return View(habitacion);

            await _servicio.Actualizar(habitacion);
            return RedirectToAction("Index", new { propiedadId = habitacion.Id_propiedad });
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(string id)
        {
            var habitacion = await _servicio.ObtenerPorId(id);
            if (habitacion == null || !await UsuarioEsPropietario(habitacion.Id_propiedad))
                return Forbid();

            await _servicio.Borrar(id);
            return RedirectToAction("Index", new { propiedadId = habitacion.Id_propiedad });
        }
    }
}