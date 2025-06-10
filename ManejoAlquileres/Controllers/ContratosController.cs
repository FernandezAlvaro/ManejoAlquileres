using ManejoAlquileres.Models;
using ManejoAlquileres.Models.Helpers;
using ManejoAlquileres.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ManejoAlquileres.Controllers
{
    public class ContratosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IServicioContrato _servicioContrato;
        private readonly IGeneradorIdsService _generadorIdsService;
        private readonly IServicioPago _servicioPago;

        public ContratosController(ApplicationDbContext context, IServicioContrato servicioContrato, IGeneradorIdsService generadorIdsService, IServicioPago servicioPago)
        {
            _context = context;
            _servicioContrato = servicioContrato;
            _generadorIdsService = generadorIdsService;
            _servicioPago = servicioPago;
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
            var usuarios = await _context.Usuarios.ToListAsync();
            ViewBag.Usuarios = usuarios;
            await CargarDatosParaVista();
            var propiedades = await _context.Propiedades
                .Where(p => p.Id_propiedad != null && p.Referencia_catastral != null)
                .ToListAsync();

            if (propiedades == null || !propiedades.Any())
            {
                TempData["Error"] = "No hay propiedades disponibles para seleccionar.";
                propiedades = new List<Propiedad>();
            }

            ViewBag.Propiedades = propiedades;
            ViewBag.Usuarios = await _context.Usuarios
                   .Select(u => new
                   {
                       u.Id_usuario,
                       u.NIF,
                       NombreCompleto = u.NIF + " - " + u.Nombre + " " + u.Apellidos
                   })
                   .ToListAsync();
            return View(new ContratoViewModel()
            {
                Fecha_inicio = DateTime.Now,
                Fecha_fin = DateTime.Now.AddYears(1),
                Fecha_max_revision = DateTime.Now.AddMonths(11),
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ContratoViewModel viewModel, bool prorroga_tacita)
        {
            if (viewModel.Propiedad == null || viewModel.Propiedad.Id_propiedad == null)
            {
                ModelState.AddModelError("Propiedad", "Debes seleccionar una propiedad válida.");
                await CargarDatosParaVista();
                return View(viewModel);
            }

            var usuarioActualId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var propietariosRelacionados = await _context.PropiedadesUsuarios
                .Where(pu => pu.PropiedadId == viewModel.Propiedad.Id_propiedad)
                .Select(pu => pu.UsuarioId)
                .ToListAsync();

            var inquilino = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NIF == viewModel.Inquilinos.FirstOrDefault().NIF);

            if (inquilino == null)
            {
                ModelState.AddModelError("Inquilino", "El inquilino debe ser un usuario registrado en la aplicación.");
                await CargarDatosParaVista();
                return View(viewModel);
            }

            if (propietariosRelacionados.Contains(inquilino.Id_usuario))
            {
                ModelState.AddModelError("Inquilino", "No puedes asignar como inquilino a un propietario de la misma propiedad.");
                await CargarDatosParaVista();
                return View(viewModel);
            }
            if (viewModel.Habitacion?.Id_habitacion != null)
            {
                if (!_context.Habitaciones.Any(h => h.Id_habitacion == viewModel.Habitacion.Id_habitacion && h.Id_propiedad == viewModel.Propiedad.Id_propiedad))
                {
                    ModelState.AddModelError("Habitacion.Id_habitacion", "La habitación no pertenece a la propiedad seleccionada.");
                }
            }


            var nuevoIdContrato = await _generadorIdsService.GenerarIdUnicoAsync();

            var contrato = new Contrato
            {
                Id_contrato = nuevoIdContrato,
                Fecha_inicio = viewModel.Fecha_inicio,
                Fecha_fin = viewModel.Fecha_fin,
                Fecha_max_revision = viewModel.Fecha_max_revision,
                Tipo_Alquiler = viewModel.Tipo_Alquiler,
                Porcentaje_incremento = viewModel.Porcentaje_incremento,
                Fianza = viewModel.Fianza,
                Clausula_prorroga = prorroga_tacita ? "Tacita" : "Convencional",
                Comision_inmobiliaria = viewModel.Comision_inmobiliaria,
                Periodicidad = viewModel.Periodicidad,
                Aval = viewModel.Aval,
                Propiedad = viewModel.Propiedad,
                Habitacion = viewModel.Habitacion,
                Inquilinos = new List<ContratoInquilino>
                {
                    new ContratoInquilino { ContratoId = nuevoIdContrato, UsuarioId = inquilino.Id_usuario }
                },
                Propietarios = propietariosRelacionados.Select(propId => new ContratoPropietario
                {
                    ContratoId = nuevoIdContrato,
                    UsuarioId = propId
                }).ToList(),
                Pagos = GenerarPagos(viewModel.Fecha_inicio, viewModel.Fecha_fin, viewModel.Periodicidad, nuevoIdContrato, viewModel.Monto_pago)
            };

            await _servicioContrato.Crear(contrato);

            TempData["Mensaje"] = "Contrato creado correctamente.";
            TempData["TipoMensaje"] = "success";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Editar(string id)
        {
            if (id == null) return NotFound();

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
                Porcentaje_incremento = contrato.Porcentaje_incremento,
                Fianza = contrato.Fianza,
                Clausula_prorroga = contrato.Clausula_prorroga,
                Comision_inmobiliaria = contrato.Comision_inmobiliaria,
                Periodicidad = contrato.Periodicidad,
                Aval = contrato.Aval,
                Propiedad = contrato.Propiedad,
                Habitacion = contrato.Habitacion,
                Inquilinos = contrato.Inquilinos.Select(i => i.Usuario).ToList(),
                Propietarios = contrato.Propietarios.Select(p => p.Usuario).ToList(),
                Pagos = contrato.Pagos,
                ContratosInquilinos = contrato.Inquilinos,
                ContratosPropietarios = contrato.Propietarios,
                PropiedadesUsuarios = await _context.PropiedadesUsuarios.Where(pu => pu.PropiedadId == contrato.Id_propiedad).ToListAsync()
            };

            var primerInquilino = contrato.Inquilinos.FirstOrDefault()?.Usuario;
            ViewBag.EsPropietario = primerInquilino != null && contrato.Propietarios.Any(p => p.Usuario.NIF == primerInquilino.NIF);

            await CargarDatosParaVista();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(string id, ContratoViewModel viewModel)
        {
            if (id != viewModel.Id_contrato) return BadRequest();

            if (!ModelState.IsValid)
            {
                await CargarDatosParaVista();
                return View(viewModel);
            }

            var contrato = await _servicioContrato.ObtenerPorId(id);
            if (contrato == null) return NotFound();

            contrato.Fecha_inicio = viewModel.Fecha_inicio;
            contrato.Fecha_fin = viewModel.Fecha_fin;
            contrato.Fecha_max_revision = viewModel.Fecha_max_revision;
            contrato.Tipo_Alquiler = viewModel.Tipo_Alquiler;
            contrato.Porcentaje_incremento = viewModel.Porcentaje_incremento;
            contrato.Fianza = viewModel.Fianza;
            contrato.Clausula_prorroga = viewModel.Clausula_prorroga;
            contrato.Comision_inmobiliaria = viewModel.Comision_inmobiliaria;
            contrato.Periodicidad = viewModel.Periodicidad;
            contrato.Aval = viewModel.Aval;

            _context.Update(contrato);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Contrato actualizado correctamente.";
            TempData["TipoMensaje"] = "success";

            return RedirectToAction(nameof(Index));
        }

        private List<Pago> GenerarPagos(DateTime inicio, DateTime fin, string periodicidad, string contratoId, decimal montoPago)
        {
            var pagos = new List<Pago>();
            var fechaActual = inicio;

            TimeSpan intervalo = periodicidad switch
            {
                "Semanal" => TimeSpan.FromDays(7),
                "Dos Semanas" => TimeSpan.FromDays(14),
                "Mensual" => TimeSpan.FromDays(30),
                "Trimestral" => TimeSpan.FromDays(90),
                "Anual" => TimeSpan.FromDays(365),
                _ => throw new ArgumentException("Periodicidad inválida")
            };

            while (fechaActual < fin)
            {
                pagos.Add(new Pago
                {
                    Id_pago = Guid.NewGuid().ToString(),
                    Id_contrato = contratoId,
                    Fecha_pago_programada = fechaActual,
                    Monto_pago = montoPago
                });

                fechaActual = fechaActual.Add(intervalo);
            }

            return pagos;
        }

        private async Task CargarDatosParaVista()
        {
            ViewBag.Propiedades = await _context.Propiedades.ToListAsync();
            ViewBag.Habitaciones = await _context.Habitaciones.ToListAsync();
            var usuarios = await _context.Usuarios.ToListAsync();
            ViewBag.Usuarios = usuarios;
            ViewBag.Periodicidades = new List<string> { "Semanal", "Dos Semanas", "Mensual", "Trimestral", "Anual" };
            ViewBag.TiposAlquiler = new List<string> { "Vivienda Habitual", "Vacacional", "Estudiante", "Otro" };
        }
    }
}