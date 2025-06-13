using System.Security.Claims;
using ManejoAlquileres.Models;
using ManejoAlquileres.Models.Helpers;
using ManejoAlquileres.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace ManejoAlquileres.Controllers
{
    [Authorize(Policy = "UsuarioAutenticado")]
    public class PropiedadesController : Controller
    {
        private readonly IServicioPropiedades _servicioPropiedades;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGeneradorIdsService _generadorIdsService;

        public PropiedadesController(
            IServicioPropiedades servicioPropiedades,
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IGeneradorIdsService generadorIdsService)
        {
            _servicioPropiedades = servicioPropiedades;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _generadorIdsService = generadorIdsService;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = ObtenerUsuarioId();

            var propiedadesPropias = await _context.PropiedadesUsuarios
                .Where(pu => pu.UsuarioId == usuarioId)
                .Include(pu => pu.Propiedad)
                    .ThenInclude(p => p.Habitaciones)
                .Include(pu => pu.Propiedad)
                    .ThenInclude(p => p.GastoInmueble)
                .Select(pu => MapearPropiedadAViewModel(pu.Propiedad, pu.PorcentajePropiedad))
                .ToListAsync();

            var propiedadesAlquiladas = await _context.ContratosInquilinos
                .Where(ci => ci.UsuarioId == usuarioId)
                .Include(ci => ci.Contrato)
                    .ThenInclude(c => c.Propiedad)
                .Select(ci => new PropiedadAlquiladaViewModel
                {
                    PropiedadId = ci.Contrato.Id_propiedad,
                    Direccion = ci.Contrato.Propiedad.Direccion,
                    TipoAlquiler = ci.Contrato.Tipo_Alquiler,
                    FechaInicio = ci.Contrato.Fecha_inicio,
                    FechaFin = ci.Contrato.Fecha_fin,
                    Periodicidad = ci.Contrato.Periodicidad,
                    Fianza = ci.Contrato.Fianza,
                    EsHabitacion = ci.Contrato.Id_habitacion != null
                })
                .ToListAsync();

            var viewModel = new PropiedadesIndexViewModel
            {
                PropiedadesPropias = propiedadesPropias,
                PropiedadesAlquiladas = propiedadesAlquiladas
            };

            return View(viewModel);
        }

        public IActionResult Crear()
        {
            var modelo = new PropiedadViewModel
            {
                FechaAdquisicion = DateTime.Today
            };

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(PropiedadViewModel vm)
        {
            if (vm.PorcentajePropiedad < 0 || vm.PorcentajePropiedad > 100)
            {
                ModelState.AddModelError(nameof(vm.PorcentajePropiedad), "El porcentaje debe estar entre 0 y 100.");
                return View(vm);
            }

            if (!ModelState.IsValid)
                return View(vm);

            var usuarioId = ObtenerUsuarioId();

            var propiedadExistente = await _context.Propiedades
                .Include(p => p.Usuarios)
                .FirstOrDefaultAsync(p => p.Referencia_catastral == vm.ReferenciaCatastral);

            if (propiedadExistente != null)
            {
                var relacionExistente = propiedadExistente.Usuarios
                    .FirstOrDefault(r => r.UsuarioId == usuarioId);

                if (relacionExistente != null)
                {
                    ModelState.AddModelError(string.Empty, "Ya tienes una relación con esta propiedad.");
                    return View(vm);
                }

                decimal sumaPorcentajes = propiedadExistente.Usuarios.Sum(r => r.PorcentajePropiedad);
                decimal nuevoPorcentaje = (decimal)(vm.PorcentajePropiedad);

                if (sumaPorcentajes + nuevoPorcentaje > 1)
                {
                    ModelState.AddModelError(string.Empty,
                        $"La suma de porcentajes de propiedad excede 100%. Actualmente está en {sumaPorcentajes}%. Por favor ajuste el porcentaje.");
                    return View(vm);
                }

                var nuevaRelacion = new PropiedadUsuario
                {
                    PropiedadId = propiedadExistente.Id_propiedad,
                    UsuarioId = usuarioId,
                    PorcentajePropiedad = nuevoPorcentaje
                };

                _context.PropiedadesUsuarios.Add(nuevaRelacion);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] = "Relación con propiedad existente creada correctamente.";
                return RedirectToAction(nameof(Index));
            }

            var propiedadNueva = new Propiedad
            {
                Id_propiedad = await _generadorIdsService.GenerarIdUnicoAsync(),
                Direccion = vm.Direccion,
                Referencia_catastral = vm.ReferenciaCatastral,
                numHabitaciones = vm.NumHabitaciones,
                Valor_catastral_piso = vm.ValorCatastralPiso,
                Valor_catastral_terreno = vm.ValorCatastralTerreno,
                Fecha_adquisicion = vm.FechaAdquisicion,
                Valor_adquisicion = vm.ValorAdquisicion,
                Valor_adqui_total = vm.ValorAdquisicionTotal,
                Estado_propiedad = vm.EstadoPropiedad,
                Descripcion = vm.Descripcion,
                Habitaciones = new List<Habitacion>(),
                GastoInmueble = new List<GastoInmueble>()
            };

            _context.Propiedades.Add(propiedadNueva);

            var relacionNueva = new PropiedadUsuario
            {
                PropiedadId = propiedadNueva.Id_propiedad,
                UsuarioId = usuarioId,
                PorcentajePropiedad = (decimal)(vm.PorcentajePropiedad)
            };

            _context.PropiedadesUsuarios.Add(relacionNueva);

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Propiedad creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Editar(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var propiedad = await _context.Propiedades
                .Include(p => p.Usuarios)
                .FirstOrDefaultAsync(p => p.Id_propiedad == id);

            if (propiedad == null)
                return NotFound();

            var usuarioId = ObtenerUsuarioId();

            var relacion = propiedad.Usuarios.FirstOrDefault(r => r.UsuarioId == usuarioId);

            if (relacion == null)
                return Forbid();

            var vm = MapearPropiedadAViewModel(propiedad, relacion.PorcentajePropiedad);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(string id, PropiedadViewModel vm)
        {
            if (id != vm.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(vm);

            var propiedad = await _context.Propiedades
                .Include(p => p.Usuarios)
                .FirstOrDefaultAsync(p => p.Id_propiedad == id);

            if (propiedad == null)
                return NotFound();

            var usuarioId = ObtenerUsuarioId();

            var relacion = propiedad.Usuarios.FirstOrDefault(r => r.UsuarioId == usuarioId);
            if (relacion == null)
                return Forbid();

            bool existeRef = await _context.Propiedades
                .AnyAsync(p => p.Referencia_catastral == vm.ReferenciaCatastral && p.Id_propiedad != id);

            if (existeRef)
            {
                ModelState.AddModelError(nameof(vm.ReferenciaCatastral), "La referencia catastral ya está registrada en otra propiedad.");
                return View(vm);
            }

            decimal nuevoPorcentaje = (decimal)(vm.PorcentajePropiedad);
            var sumaOtrosPorcentajes = propiedad.Usuarios
                .Where(r => r.UsuarioId != usuarioId)
                .Sum(r => r.PorcentajePropiedad);

            if (nuevoPorcentaje + sumaOtrosPorcentajes > 1)
            {
                ModelState.AddModelError(nameof(vm.PorcentajePropiedad), $"La suma de porcentajes excede 100%. Actualmente está en {sumaOtrosPorcentajes}% para otros usuarios.");
                return View(vm);
            }

            relacion.PorcentajePropiedad = nuevoPorcentaje;
            _context.PropiedadesUsuarios.Update(relacion);

            ActualizarPropiedadDesdeViewModel(propiedad, vm);
            _context.Propiedades.Update(propiedad);

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Propiedad actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detalles(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var propiedad = await _context.Propiedades
                .Include(p => p.Habitaciones)
                .Include(p => p.GastoInmueble)
                .Include(p => p.Usuarios)
                .FirstOrDefaultAsync(p => p.Id_propiedad == id);

            if (propiedad == null)
                return NotFound();

            var usuarioId = ObtenerUsuarioId();

            var relacion = propiedad.Usuarios.FirstOrDefault(r => r.UsuarioId == usuarioId);

            if (relacion == null)
                return Forbid();

            var vm = MapearPropiedadAViewModel(propiedad, relacion.PorcentajePropiedad);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrar(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var propiedad = await _context.Propiedades
                .Include(p => p.Usuarios)
                .Include(p => p.Habitaciones)
                .Include(p => p.GastoInmueble)
                .FirstOrDefaultAsync(p => p.Id_propiedad == id);

            if (propiedad == null)
                return NotFound();

            var usuarioId = ObtenerUsuarioId();

            var relacion = propiedad.Usuarios.FirstOrDefault(r => r.UsuarioId == usuarioId);

            if (relacion == null)
                return Forbid();

            if (propiedad.Usuarios.Count == 1)
            {
                // Sólo hay esta relación: eliminar propiedad, gastos y habitaciones
                _context.Habitaciones.RemoveRange(propiedad.Habitaciones);
                _context.GastosInmueble.RemoveRange(propiedad.GastoInmueble);
                _context.PropiedadesUsuarios.Remove(relacion);
                _context.Propiedades.Remove(propiedad);
            }
            else
            {
                // Hay otras relaciones: sólo eliminar esta relación
                _context.PropiedadesUsuarios.Remove(relacion);
            }

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Propiedad o relación eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> ConfirmarBorrado(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var propiedad = await _context.Propiedades
                .FirstOrDefaultAsync(p => p.Id_propiedad == id);

            if (propiedad == null)
                return NotFound();

            var usuarioId = ObtenerUsuarioId();

            var relacion = await _context.PropiedadesUsuarios
                .FirstOrDefaultAsync(r => r.PropiedadId == id && r.UsuarioId == usuarioId);

            if (relacion == null)
                return Forbid();

            var vm = new PropiedadViewModel
            {
                Id = propiedad.Id_propiedad,
                Direccion = propiedad.Direccion,
                ReferenciaCatastral = propiedad.Referencia_catastral,
                EstadoPropiedad = propiedad.Estado_propiedad,
                ValorAdquisicion = propiedad.Valor_adquisicion,
                Descripcion = propiedad.Descripcion
            };

            return View(vm); // => Esto debe abrir la vista "ConfirmarBorrado.cshtml"
        }

        // Acción para crear habitación (botón en la tabla propiedades)
        public async Task<IActionResult> CrearHabitacion(string propiedadId)
        {
            if (string.IsNullOrWhiteSpace(propiedadId))
                return BadRequest();

            var propiedad = await _context.Propiedades
                .Include(p => p.Usuarios)
                .FirstOrDefaultAsync(p => p.Id_propiedad == propiedadId);

            if (propiedad == null)
                return NotFound();

            var usuarioId = ObtenerUsuarioId();

            var relacion = propiedad.Usuarios.FirstOrDefault(r => r.UsuarioId == usuarioId);

            if (relacion == null)
                return Forbid();

            var habitacion = new Habitacion { Id_propiedad = propiedadId };

            return View(habitacion); // Mostrar formulario para crear habitación
        }

        // POST: Crear habitación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearHabitacion(Habitacion habitacion)
        {
            var propiedadId = habitacion.Id_propiedad;
            habitacion.Id_habitacion = await _generadorIdsService.GenerarIdUnicoAsync();

            var propiedad = await _context.Propiedades
                .Include(p => p.Habitaciones)
                .Include(p => p.Usuarios)
                .FirstOrDefaultAsync(p => p.Id_propiedad == propiedadId);

            if (propiedad == null)
                return NotFound();

            var usuarioId = ObtenerUsuarioId();

            var relacion = propiedad.Usuarios.FirstOrDefault(r => r.UsuarioId == usuarioId);

            if (relacion == null)
                return Forbid();

            if (propiedad.Habitaciones.Count >= propiedad.numHabitaciones)
            {
                TempData["Error"] = "No se pueden agregar más habitaciones; se alcanzó el límite definido en la propiedad.";
                habitacion.Id_habitacion = await _generadorIdsService.GenerarIdUnicoAsync();
                habitacion.Id_propiedad = propiedadId;
                return RedirectToAction(nameof(Detalles), new { id = propiedadId });
            }
            else
            {
                habitacion.Id_habitacion = await _generadorIdsService.GenerarIdUnicoAsync();
                habitacion.Id_propiedad = propiedadId;

                _context.Habitaciones.Add(habitacion);
                await _context.SaveChangesAsync();
                TempData["MensajeExito"] = "Habitación creada correctamente.";
                return RedirectToAction(nameof(Detalles), new { id = propiedadId });

            }
        }

        public async Task<IActionResult> CrearGasto(string propiedadId)
        {
            if (string.IsNullOrWhiteSpace(propiedadId))
                return BadRequest();

            var propiedad = await _context.Propiedades
                .Include(p => p.Usuarios)
                .FirstOrDefaultAsync(p => p.Id_propiedad == propiedadId);

            if (propiedad == null)
                return NotFound();

            var usuarioId = ObtenerUsuarioId();

            var relacion = propiedad.Usuarios.FirstOrDefault(r => r.UsuarioId == usuarioId);

            if (relacion == null)
                return Forbid();

            var gasto = new GastoInmueble
            {
                Id_propiedad = propiedadId,
                Fecha_pago = DateTime.Today
            };

            return View(gasto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearGasto(GastoInmueble gasto)
        {
            var propiedadId = gasto.Id_propiedad;

            if (string.IsNullOrWhiteSpace(propiedadId) || gasto == null)
                return BadRequest();

            var propiedad = await _context.Propiedades
                .Include(p => p.GastoInmueble)
                .Include(p => p.Usuarios)
                .FirstOrDefaultAsync(p => p.Id_propiedad == propiedadId);

            if (propiedad == null)
                return NotFound();

            var usuarioId = ObtenerUsuarioId();

            var relacion = propiedad.Usuarios.FirstOrDefault(r => r.UsuarioId == usuarioId);

            if (relacion == null)
                return Forbid();

            gasto.Id_gasto = await _generadorIdsService.GenerarIdUnicoAsync();
            gasto.Id_propiedad = propiedadId;
            gasto.Porcentaje_amortizacion = gasto.Porcentaje_amortizacion;

            _context.GastosInmueble.Add(gasto);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Gasto creado correctamente.";
            return RedirectToAction(nameof(Detalles), new { id = propiedadId });
        }

        [HttpGet]
        [HttpGet]
        public IActionResult VerTodas()
        {
            var propiedades = _context.Propiedades
                .Include(p => p.Habitaciones)
                .Include(p => p.Usuarios)
                    .ThenInclude(pu => pu.Usuario)
                .ToList();

            var propiedadVMs = propiedades.Select(p => new PropiedadAdminViewModel
                {
                    Id = p.Id_propiedad,
                    Direccion = p.Direccion,
                    ReferenciaCatastral = p.Referencia_catastral,
                    NumHabitaciones = p.numHabitaciones,
                    ValorCatastralPiso = p.Valor_catastral_piso,
                    ValorCatastralTerreno = p.Valor_catastral_terreno,
                    FechaAdquisicion = p.Fecha_adquisicion,
                    ValorAdquisicion = p.Valor_adquisicion,
                    ValorAdquisicionTotal = p.Valor_adqui_total,
                    EstadoPropiedad = p.Estado_propiedad,
                    Descripcion = p.Descripcion,

                    PorcentajesUsuarios = p.Usuarios.Select(u => new UsuarioPorcentajeViewModel
                    {
                        UsuarioId = u.UsuarioId,
                        NIF = u.Usuario.NIF,
                        NombreCompleto = $"{u.Usuario.Nombre} {u.Usuario.Apellidos}",
                        Porcentaje = u.PorcentajePropiedad
                    }).ToList()
                }).ToList();

            return View(propiedadVMs);
        }

        // Métodos auxiliares
        private bool EsAdmin()
        {
            var rol = _httpContextAccessor.HttpContext?.User.FindFirst("esAdministrador")?.Value;
            return rol == "true";
        }


        private string ObtenerUsuarioId()
        {
            return _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private static PropiedadViewModel MapearPropiedadAViewModel(Propiedad propiedad, decimal porcentajePropiedad)
        {
            return new PropiedadViewModel
            {
                Id = propiedad.Id_propiedad,
                Direccion = propiedad.Direccion,
                ReferenciaCatastral = propiedad.Referencia_catastral,
                NumHabitaciones = propiedad.numHabitaciones,
                ValorCatastralPiso = propiedad.Valor_catastral_piso,
                ValorCatastralTerreno = propiedad.Valor_catastral_terreno,
                FechaAdquisicion = propiedad.Fecha_adquisicion,
                ValorAdquisicion = propiedad.Valor_adquisicion,
                ValorAdquisicionTotal = propiedad.Valor_adqui_total,
                EstadoPropiedad = propiedad.Estado_propiedad,
                Descripcion = propiedad.Descripcion,
                Habitaciones = propiedad.Habitaciones,
                GastosInmueble = propiedad.GastoInmueble,
                PorcentajePropiedad = (double)(porcentajePropiedad)
            };
        }

        public async Task<IActionResult> EditarAdmin(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var propiedad = await _context.Propiedades
                    .Include(p => p.Usuarios)
                        .ThenInclude(pu => pu.Usuario)
                    .FirstOrDefaultAsync(p => p.Id_propiedad == id);

            if (propiedad == null)
                return NotFound();

            var vm = MapearPropiedadAdminAViewModel(propiedad);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarAdmin(string id, PropiedadAdminViewModel vm)
        {
            if (id != vm.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(vm);

            var propiedad = await _context.Propiedades
                .Include(p => p.Usuarios)
                .FirstOrDefaultAsync(p => p.Id_propiedad == id);

            if (propiedad == null)
                return NotFound();

            bool existeRef = await _context.Propiedades
                .AnyAsync(p => p.Referencia_catastral == vm.ReferenciaCatastral && p.Id_propiedad != id);

            if (existeRef)
            {
                ModelState.AddModelError(nameof(vm.ReferenciaCatastral), "La referencia catastral ya está registrada.");
                return View(vm);
            }
            var sumaPorcentajes = vm.PorcentajesUsuarios.Sum(u => u.Porcentaje);
            if (sumaPorcentajes > 100)
            {
                ModelState.AddModelError("", "La suma de los porcentajes de propiedad no puede superar el 100%.");
                return View(vm);
            }

            ActualizarPropiedadDesdeAdminViewModel(propiedad, vm);

            _context.PropiedadesUsuarios.RemoveRange(propiedad.Usuarios);

            
            foreach (var u in vm.PorcentajesUsuarios)
            {
                propiedad.Usuarios.Add(new PropiedadUsuario
                {
                    UsuarioId = u.UsuarioId,
                    PorcentajePropiedad = u.Porcentaje
                });
            }

            _context.Propiedades.Update(propiedad);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Propiedad actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DetallesAdmin(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var propiedad = await _context.Propiedades
                .Include(p => p.Habitaciones)
                .Include(p => p.GastoInmueble)
                .Include(p => p.Usuarios)
                .ThenInclude(pu => pu.Usuario)
                .FirstOrDefaultAsync(p => p.Id_propiedad == id);

            if (propiedad == null)
                return NotFound();

            var vm = MapearPropiedadAdminAViewModel(propiedad);

            return View(vm);
        }

        public async Task<IActionResult> ConfirmarBorradoAdmin(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var propiedad = await _context.Propiedades.FirstOrDefaultAsync(p => p.Id_propiedad == id);

            if (propiedad == null)
                return NotFound();

            var vm = new PropiedadAdminViewModel
            {
                Id = propiedad.Id_propiedad,
                Direccion = propiedad.Direccion,
                ReferenciaCatastral = propiedad.Referencia_catastral,
                EstadoPropiedad = propiedad.Estado_propiedad,
                ValorAdquisicion = propiedad.Valor_adquisicion,
                Descripcion = propiedad.Descripcion
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BorrarAdmin(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var propiedad = await _context.Propiedades
                .Include(p => p.Habitaciones)
                .Include(p => p.GastoInmueble)
                .Include(p => p.Usuarios)
                .FirstOrDefaultAsync(p => p.Id_propiedad == id);

            if (propiedad == null)
                return NotFound();

            _context.Habitaciones.RemoveRange(propiedad.Habitaciones);
            _context.GastosInmueble.RemoveRange(propiedad.GastoInmueble);
            _context.PropiedadesUsuarios.RemoveRange(propiedad.Usuarios);
            _context.Propiedades.Remove(propiedad);

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Propiedad eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> VerDetalles(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var usuarioId = ObtenerUsuarioId();

            var contratoInquilino = await _context.ContratosInquilinos
                .Include(ci => ci.Contrato)
                    .ThenInclude(c => c.Propiedad)
                .FirstOrDefaultAsync(ci => ci.UsuarioId == usuarioId && ci.Contrato.Id_propiedad == id);

            if (contratoInquilino == null)
                return Forbid();

            var propiedad = contratoInquilino.Contrato.Propiedad;

            var vm = new PropiedadViewModel
            {
                Id = propiedad.Id_propiedad,
                Direccion = propiedad.Direccion,
                ReferenciaCatastral = propiedad.Referencia_catastral,
                EstadoPropiedad = propiedad.Estado_propiedad,
                ValorAdquisicion = propiedad.Valor_adquisicion,
                ValorCatastralPiso = propiedad.Valor_catastral_piso,
                ValorCatastralTerreno = propiedad.Valor_catastral_terreno,
                FechaAdquisicion = propiedad.Fecha_adquisicion,
                Descripcion = propiedad.Descripcion,
                NumHabitaciones = propiedad.numHabitaciones
            };

            return View("VerDetalles", vm); // Asegúrate de que la vista esté en Views/Propiedades/
        }
        private static void ActualizarPropiedadDesdeViewModel(Propiedad propiedad, PropiedadViewModel vm)
        {
            propiedad.Direccion = vm.Direccion;
            propiedad.Referencia_catastral = vm.ReferenciaCatastral;
            propiedad.numHabitaciones = vm.NumHabitaciones;
            propiedad.Valor_catastral_piso = vm.ValorCatastralPiso;
            propiedad.Valor_catastral_terreno = vm.ValorCatastralTerreno;
            propiedad.Fecha_adquisicion = vm.FechaAdquisicion;
            propiedad.Valor_adquisicion = vm.ValorAdquisicion;
            propiedad.Valor_adqui_total = vm.ValorAdquisicionTotal;
            propiedad.Estado_propiedad = vm.EstadoPropiedad;
            propiedad.Descripcion = vm.Descripcion;
        }

        private PropiedadAdminViewModel MapearPropiedadAdminAViewModel(Propiedad propiedad)
        {
            return new PropiedadAdminViewModel
            {
                Id = propiedad.Id_propiedad,
                Direccion = propiedad.Direccion,
                ReferenciaCatastral = propiedad.Referencia_catastral,
                EstadoPropiedad = propiedad.Estado_propiedad,
                FechaAdquisicion = propiedad.Fecha_adquisicion,
                ValorAdquisicion = propiedad.Valor_adquisicion,
                ValorAdquisicionTotal = propiedad.Valor_adqui_total,
                ValorCatastralPiso = propiedad.Valor_catastral_piso,
                ValorCatastralTerreno = propiedad.Valor_catastral_terreno,
                NumHabitaciones = propiedad.numHabitaciones,
                Descripcion = propiedad.Descripcion,
                PorcentajesUsuarios = propiedad.Usuarios
                    .Select(u => new UsuarioPorcentajeViewModel
                    {
                        UsuarioId = u.UsuarioId,
                        NIF = u.Usuario.NIF,
                        NombreCompleto = u.Usuario.Nombre + u.Usuario.Apellidos,
                        Porcentaje = u.PorcentajePropiedad
                    }).ToList()
            };
        }

        private void ActualizarPropiedadDesdeAdminViewModel(Propiedad propiedad, PropiedadAdminViewModel vm)
        {
            propiedad.Direccion = vm.Direccion;
            propiedad.Referencia_catastral = vm.ReferenciaCatastral;
            propiedad.Estado_propiedad = vm.EstadoPropiedad;
            propiedad.Fecha_adquisicion = vm.FechaAdquisicion;
            propiedad.Valor_adquisicion = vm.ValorAdquisicion;
            propiedad.Valor_adqui_total = vm.ValorAdquisicionTotal;
            propiedad.Valor_catastral_piso = vm.ValorCatastralPiso;
            propiedad.Valor_catastral_terreno = vm.ValorCatastralTerreno;
            propiedad.numHabitaciones = vm.NumHabitaciones;
            propiedad.Descripcion = vm.Descripcion;
        }
    }
}