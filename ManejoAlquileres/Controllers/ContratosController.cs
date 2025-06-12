using ManejoAlquileres.Models;
using ManejoAlquileres.Models.Helpers;
using ManejoAlquileres.Service;
using ManejoAlquileres.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace ManejoAlquileres.Controllers
{
    [Authorize]
    public class ContratosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IServicioContrato _servicioContrato;
        private readonly IGeneradorIdsService _generadorIdsService;
        private readonly IServicioPago _servicioPago;
        private readonly IServicioUsuarios _servicioUsuarios;
        private readonly IContratoInquilino _servicioContratoInquilino;

        public ContratosController(
            ApplicationDbContext context,
            IServicioContrato servicioContrato,
            IGeneradorIdsService generadorIdsService,
            IServicioPago servicioPago,
            IServicioUsuarios servicioUsuarios,
            IContratoInquilino servicioContratoInquilino)
        {
            _context = context;
            _servicioContrato = servicioContrato;
            _generadorIdsService = generadorIdsService;
            _servicioPago = servicioPago;
            _servicioUsuarios = servicioUsuarios;
            _servicioContratoInquilino = servicioContratoInquilino;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var contratosInquilino = await _context.Contratos
                .Include(c => c.Propiedad)
                .Include(c => c.Habitacion)
                .Include(c => c.Inquilinos).ThenInclude(i => i.Usuario)
                .Where(c => c.Inquilinos.Any(i => i.UsuarioId == userId))
                .ToListAsync();

            var contratosPropietario = await _context.Contratos
                .Include(c => c.Propiedad)
                .Include(c => c.Habitacion)
                .Include(c => c.Propietarios).ThenInclude(p => p.Usuario)
                .Where(c => c.Propietarios.Any(p => p.UsuarioId == userId))
                .ToListAsync();

            ViewBag.ContratosInquilino = contratosInquilino;
            ViewBag.ContratosPropietario = contratosPropietario;

            return View();
        }

        public async Task<IActionResult> Crear()
        {
            await CargarDatosParaVista();

            var propiedades = await _context.Propiedades
                .Where(p => !string.IsNullOrEmpty(p.Id_propiedad) && !string.IsNullOrEmpty(p.Referencia_catastral))
                .ToListAsync();

            if (!propiedades.Any())
            {
                TempData["Error"] = "No hay propiedades disponibles para seleccionar.";
                propiedades = new List<Propiedad>();
            }

            ViewBag.Propiedades = propiedades;

            return View(new ContratoViewModel
            {
                Fecha_inicio = DateTime.Now,
                Fecha_fin = DateTime.Now.AddYears(1),
                Fecha_max_revision = DateTime.Now.AddMonths(11),
                Porcentaje_incremento = 0,
                Comision_inmobiliaria = 0
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ContratoViewModel viewModel)
        {
            Propiedad? propiedad = null;
            Habitacion? habitacion = null;
            List<Usuario> inquilinos = new();
            List<string> propietariosRelacionados = new();

            // Validar propiedad seleccionada
            if (viewModel.Propiedad == null || string.IsNullOrWhiteSpace(viewModel.Propiedad.Id_propiedad))
            {
                ModelState.AddModelError("Propiedad", "Debes seleccionar una propiedad válida.");
            }
            else
            {
                propiedad = await _context.Propiedades
                    .FirstOrDefaultAsync(p => p.Id_propiedad == viewModel.Propiedad.Id_propiedad);

                if (propiedad == null)
                {
                    ModelState.AddModelError("Propiedad", "La propiedad seleccionada no existe.");
                }
                viewModel.Propiedad = propiedad;
                viewModel.Propiedad.Referencia_catastral = propiedad.Referencia_catastral;
                viewModel.Propiedad.Direccion = propiedad.Direccion;
            }

            // Sólo si la propiedad es válida seguimos validando inquilinos y habitación
            if (propiedad != null)
            {
                // Validar inquilinos seleccionados
                if (viewModel.InquilinosSeleccionados == null || !viewModel.InquilinosSeleccionados.Any())
                {
                    ModelState.AddModelError("Inquilino", "Debes seleccionar al menos un inquilino.");
                }
                else
                {
                    inquilinos = await _context.Usuarios
                        .Where(u => viewModel.InquilinosSeleccionados.Contains(u.Id_usuario))
                        .ToListAsync();

                    if (!inquilinos.Any())
                    {
                        ModelState.AddModelError("Inquilino", "El inquilino debe ser un usuario registrado en la aplicación.");
                    }
                    else
                    {
                        // Obtener propietarios relacionados a la propiedad
                        propietariosRelacionados = await _context.PropiedadesUsuarios
                            .Where(pu => pu.PropiedadId == propiedad.Id_propiedad)
                            .Select(pu => pu.UsuarioId)
                            .ToListAsync();

                        // Validar que ningún inquilino sea propietario
                        if (inquilinos.Any(i => propietariosRelacionados.Contains(i.Id_usuario)))
                        {
                            ModelState.AddModelError("Inquilino", "No puedes asignar como inquilino a un propietario de la misma propiedad.");
                        }
                    }
                }

                // Validar habitación si fue seleccionada
                if (viewModel.Habitacion?.Id_habitacion != null)
                {
                    habitacion = await _context.Habitaciones
                        .FirstOrDefaultAsync(h => h.Id_habitacion == viewModel.Habitacion.Id_habitacion
                                               && h.Id_propiedad == propiedad.Id_propiedad);

                    if (habitacion == null)
                    {
                        ModelState.AddModelError("Habitacion", "La habitación no pertenece a la propiedad seleccionada.");
                    }
                    viewModel.Habitacion = habitacion;
                }
            }
            ModelState.Remove("Habitacion.Id_habitacion");
            ModelState.Remove("Habitacion.Id_propiedad");
            ModelState.Remove("Propiedad.Id_propiedad");
            ModelState.Remove("Propiedad.Referencia_catastral");
            ModelState.Remove("Propiedad.Direccion");
            if (!ModelState.IsValid)
            {
                await CargarDatosParaVista();
                ViewBag.Propiedades = propiedad != null ? new List<Propiedad> { propiedad } : new List<Propiedad>();
                return View(viewModel);
            }

            var nuevoIdContrato = await _generadorIdsService.GenerarIdUnicoAsync();

            var contrato = new Contrato
            {
                Id_contrato = nuevoIdContrato,
                Fecha_inicio = viewModel.Fecha_inicio,
                Fecha_fin = viewModel.Fecha_fin,
                Fecha_max_revision = viewModel.Fecha_max_revision,
                Tipo_Alquiler = viewModel.Tipo_Alquiler,
                Porcentaje_incremento = (viewModel.Porcentaje_incremento) / 100m,
                Fianza = viewModel.Fianza,
                Clausula_prorroga = viewModel.Clausula_prorroga,
                Comision_inmobiliaria = (viewModel.Comision_inmobiliaria) / 100m,
                Periodicidad = viewModel.Periodicidad,
                Aval = viewModel.Aval,
                Propiedad = propiedad,
                Habitacion = habitacion,
                Inquilinos = inquilinos.Select(i => new ContratoInquilino
                {
                    ContratoId = nuevoIdContrato,
                    UsuarioId = i.Id_usuario
                }).ToList(),
                Propietarios = propietariosRelacionados.Select(pid => new ContratoPropietario
                {
                    ContratoId = nuevoIdContrato,
                    UsuarioId = pid
                }).ToList(),
                Pagos = await GenerarPagosAsync(viewModel.Fecha_inicio, viewModel.Fecha_fin, viewModel.Periodicidad, nuevoIdContrato, viewModel.Monto_pago)
            };

            await _servicioContrato.Crear(contrato);

            TempData["Mensaje"] = "Contrato creado correctamente.";
            TempData["TipoMensaje"] = "success";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Editar(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var contrato = await _servicioContrato.ObtenerPorId(id);
            if (contrato == null) return NotFound();

            var viewModel = new ContratoViewModel
            {
                Id_contrato = contrato.Id_contrato,
                Fecha_inicio = contrato.Fecha_inicio,
                Fecha_fin = contrato.Fecha_fin,
                Fecha_max_revision = contrato.Fecha_max_revision,
                Tipo_Alquiler = contrato.Tipo_Alquiler,
                Monto_pago = contrato.Pagos.FirstOrDefault()?.Monto_pago ?? contrato.Fianza,
                Porcentaje_incremento = contrato.Porcentaje_incremento * 100m,
                Fianza = contrato.Fianza,
                Clausula_prorroga = contrato.Clausula_prorroga,
                Comision_inmobiliaria = contrato.Comision_inmobiliaria * 100m,
                Periodicidad = contrato.Periodicidad,
                Aval = contrato.Aval,
                Propiedad = contrato.Propiedad,
                Habitacion = contrato.Habitacion,
                Inquilinos = contrato.Inquilinos.Select(i => i.Usuario).ToList(),
                Propietarios = contrato.Propietarios.Select(p => p.Usuario).ToList(),
                Pagos = contrato.Pagos,
                ContratosInquilinos = contrato.Inquilinos,
                ContratosPropietarios = contrato.Propietarios,
                PropiedadesUsuarios = await _context.PropiedadesUsuarios.Where(pu => pu.PropiedadId == contrato.Propiedad.Id_propiedad).ToListAsync()
            };

            var primerInquilino = contrato.Inquilinos.FirstOrDefault()?.Usuario;
            ViewBag.EsPropietario = primerInquilino != null && contrato.Propietarios.Any(p => p.Usuario.NIF == primerInquilino.NIF);

            await CargarDatosParaVista();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(string id, ContratoViewModel viewModel)
        {
            if (id != viewModel.Id_contrato)
            {
                ModelState.AddModelError("", "El identificador del contrato no coincide.");
            }

            if (viewModel.Propiedad == null || string.IsNullOrEmpty(viewModel.Propiedad.Id_propiedad))
            {
                ModelState.AddModelError("Propiedad", "Debes seleccionar una propiedad válida.");
            }

            var propiedad = await _context.Propiedades.FirstOrDefaultAsync(p => p.Id_propiedad == viewModel.Propiedad.Id_propiedad);
            if (propiedad == null)
            {
                ModelState.AddModelError("Propiedad", "La propiedad seleccionada no existe.");
            }
            else
                viewModel.Propiedad = propiedad;
            if (viewModel.Habitacion?.Id_habitacion != null)
            {
                var habitacionesDePropiedad = await _context.Habitaciones
                    .Where(h => h.Id_propiedad == viewModel.Propiedad.Id_propiedad)
                    .ToListAsync();

                ViewBag.Habitaciones = habitacionesDePropiedad.Select(h => new
                {
                    Id_habitacion = h.Id_habitacion,
                    Descripcion = h.Descripcion
                }).ToList();

                bool habitacionValida = habitacionesDePropiedad
                    .Any(h => h.Id_habitacion == viewModel.Habitacion.Id_habitacion);

                if (!habitacionValida)
                {
                    ModelState.AddModelError("Habitacion", "La habitación no pertenece a la propiedad seleccionada.");
                }
            }

            var inquilinos = new List<Usuario>();
            if (viewModel.InquilinosSeleccionados != null && viewModel.InquilinosSeleccionados.Any())
            {
                inquilinos = await _context.Usuarios
                    .Where(u => viewModel.InquilinosSeleccionados.Contains(u.Id_usuario))
                    .ToListAsync();

                if (!inquilinos.Any())
                {
                    ModelState.AddModelError("Inquilino", "Debes seleccionar al menos un inquilino válido.");
                }
            }
            else
            {
                ModelState.AddModelError("Inquilino", "Debes seleccionar al menos un inquilino válido.");
            }

            var propietariosRelacionados = new List<string>();
            if (propiedad != null)
            {
                propietariosRelacionados = await _context.PropiedadesUsuarios
                    .Where(pu => pu.PropiedadId == propiedad.Id_propiedad)
                    .Select(pu => pu.UsuarioId)
                    .ToListAsync();

                if (inquilinos.Any(i => propietariosRelacionados.Contains(i.Id_usuario)))
                {
                    ModelState.AddModelError("Inquilino", "No puedes asignar como inquilino a un propietario de la misma propiedad.");
                }
            }

            if (viewModel.Fecha_inicio == default || viewModel.Fecha_fin == default)
            {
                var contratoExistente = await _servicioContrato.ObtenerPorId(id);
                if (contratoExistente != null)
                {
                    viewModel.Fecha_inicio = contratoExistente.Fecha_inicio;
                    viewModel.Fecha_fin = contratoExistente.Fecha_fin;
                }
            }

            ModelState.Remove("Habitacion.Id_habitacion");
            ModelState.Remove("Habitacion.Id_propiedad");
            ModelState.Remove("Propiedad.Id_propiedad");
            ModelState.Remove("Propiedad.Referencia_catastral");
            ModelState.Remove("Propiedad.Direccion");
            ModelState.Remove("Inquilino.Email");
            ModelState.Remove("Inquilino.Nombre");
            ModelState.Remove("Inquilino.Telefono");
            ModelState.Remove("Inquilino.Apellidos");
            ModelState.Remove("Inquilino.Contraseña");
            ModelState.Remove("Inquilino.Direccion");
            ModelState.Remove("Inquilino.Informacion_bancaria");
            if (!ModelState.IsValid)
            {
                await CargarDatosParaVista();
                return View(viewModel);
            }

            var contrato = await _servicioContrato.ObtenerPorId(id);
            if (contrato == null)
            {
                ModelState.AddModelError("", "Contrato no encontrado.");
                await CargarDatosParaVista();
                return View(viewModel);
            }

            contrato.Fecha_inicio = viewModel.Fecha_inicio;
            contrato.Fecha_fin = viewModel.Fecha_fin;
            contrato.Fecha_max_revision = viewModel.Fecha_max_revision;
            contrato.Tipo_Alquiler = viewModel.Tipo_Alquiler;
            contrato.Porcentaje_incremento = viewModel.Porcentaje_incremento / 100m;
            contrato.Fianza = viewModel.Fianza;
            contrato.Clausula_prorroga = viewModel.Clausula_prorroga;
            contrato.Comision_inmobiliaria = viewModel.Comision_inmobiliaria / 100m;
            contrato.Periodicidad = viewModel.Periodicidad;
            contrato.Aval = viewModel.Aval;
            contrato.Propiedad = propiedad;
            contrato.Habitacion = viewModel.Habitacion?.Id_habitacion != null
                ? await _context.Habitaciones.FindAsync(viewModel.Habitacion.Id_habitacion)
                : null;

            await _servicioContratoInquilino.DeleteByContratoIdAsync(id);

            var nuevosInquilinos = viewModel.InquilinosSeleccionados.Select(uId => new ContratoInquilino
            {
                ContratoId = id,
                UsuarioId = uId
            }).ToList();

            await _servicioContratoInquilino.AddRangeAsync(nuevosInquilinos);

            contrato.Propietarios = propietariosRelacionados.Select(p => new ContratoPropietario
            {
                ContratoId = contrato.Id_contrato,
                UsuarioId = p
            }).ToList();

            _context.Update(contrato);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Contrato actualizado correctamente.";
            TempData["TipoMensaje"] = "success";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult VerTodosLosContratos()
        {
            var contratos = _context.Contratos.Include(c => c.Propiedad).ToList();
            return View(contratos);
        }

        public async Task<IActionResult> Detalles(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var contrato = await _context.Contratos
                .Include(c => c.Propiedad)
                .Include(c => c.Habitacion)
                .Include(c => c.Inquilinos).ThenInclude(i => i.Usuario)
                .Include(c => c.Propietarios).ThenInclude(p => p.Usuario)
                .Include(c => c.Pagos)
                .FirstOrDefaultAsync(c => c.Id_contrato == id);

            if (contrato == null)
                return NotFound();

            return View(contrato);
        }

        [Authorize]
        public async Task<IActionResult> Eliminar(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var contrato = await _context.Contratos
                .Include(c => c.Propiedad)
                .Include(c => c.Inquilinos).ThenInclude(i => i.Usuario)
                .Include(c => c.Propietarios).ThenInclude(p => p.Usuario)
                .FirstOrDefaultAsync(c => c.Id_contrato == id);

            if (contrato == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Administrador");

            bool puedeEliminar = isAdmin ||
                                 contrato.Inquilinos.Any(i => i.UsuarioId == userId) ||
                                 contrato.Propietarios.Any(p => p.UsuarioId == userId);

            if (!puedeEliminar)
                return Forbid();

            return View(contrato); // Confirmación
        }

        // POST: Contratos/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> EliminarConfirmado(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var contrato = await _context.Contratos
                .Include(c => c.Inquilinos)
                .Include(c => c.Propietarios)
                .FirstOrDefaultAsync(c => c.Id_contrato == id);

            if (contrato == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Administrador");

            bool puedeEliminar = isAdmin ||
                                 contrato.Inquilinos.Any(i => i.UsuarioId == userId) ||
                                 contrato.Propietarios.Any(p => p.UsuarioId == userId);

            if (!puedeEliminar)
                return Forbid();

            _context.Contratos.Remove(contrato);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(VerTodosLosContratos));
        }

        private async Task<List<Pago>> GenerarPagosAsync(DateTime fechaInicio, DateTime fechaFin, string periodicidad, string contratoId, decimal monto)
        {
            var pagos = new List<Pago>();
            var fecha = fechaInicio;
            var intervalo = periodicidad switch
            {
                "Semanal" => TimeSpan.FromDays(7),
                "Dos Semanas" => TimeSpan.FromDays(14),
                "Mensual" => TimeSpan.FromDays(30), // Puedes mejorar esto con fecha real del mes siguiente
                "Trimestral" => TimeSpan.FromDays(90),
                "Anual" => TimeSpan.FromDays(365),
                _ => throw new ArgumentException("Periodicidad no válida")
            };

            while (fecha <= fechaFin)
            {
                pagos.Add(new Pago
                {
                    Id_pago = await _generadorIdsService.GenerarIdUnicoAsync(),
                    Id_contrato = contratoId,
                    Fecha_pago_programada = fecha,
                    Monto_pago = monto,
                    Fecha_pago_real = new DateTime(0001, 1, 1)
                });

                fecha = fecha.Add(intervalo);
            }

            return pagos;
        }


        private async Task CargarDatosParaVista(string propiedadId = null)
        {
            ViewBag.Propiedades = await _context.Propiedades.ToListAsync();

            if (!string.IsNullOrEmpty(propiedadId))
            {
                ViewBag.Habitaciones = await _context.Habitaciones
                    .Where(h => h.Id_propiedad == propiedadId)
                    .ToListAsync();
            }
            else
            {
                ViewBag.Habitaciones = new List<Habitacion>();
            }

            ViewBag.Usuarios = await _context.Usuarios
                .Select(u => new
                {
                    u.Id_usuario,
                    u.NIF,
                    NombreCompleto = $"{u.NIF} - {u.Nombre} {u.Apellidos}"
                }).ToListAsync();

            ViewBag.Periodicidades = new List<string> { "Semanal", "Dos Semanas", "Mensual", "Trimestral", "Anual" };
            ViewBag.TiposAlquiler = new List<string> { "Vivienda Habitual", "Vacacional", "Estudiante", "Otro" };
        }
        [HttpGet]
        public async Task<JsonResult> ObtenerHabitacionesPorPropiedad(string propiedadId)
        {
            if (string.IsNullOrEmpty(propiedadId))
                return Json(new List<object>());

            var habitaciones = await _context.Habitaciones
                .Where(h => h.Id_propiedad == propiedadId)
                .Select(h => new { h.Id_habitacion, h.Descripcion })
                .ToListAsync();

            return Json(habitaciones);
        }

    }
}