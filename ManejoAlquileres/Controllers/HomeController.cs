using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using ManejoAlquileres.Models;
using ManejoAlquileres.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ClosedXML.Excel;
using ManejoAlquileres.Service;


namespace ManejoAlquileres.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;
        private readonly IServicioPago _servicioPago;
        private readonly IHtmlToPdfConverter _pdfConverter;
        private readonly IServicioUsuarios _servicioUsuarios;
        private readonly IServicioPropiedades _servicioPropiedades;
        private readonly IPropiedadUsuarioService _propiedadUsuario;
        private readonly IServicioGastoInmueble _servicioGastoInmueble;
        private readonly IServicioContrato _servicioContrato;

        public HomeController(
            ILogger<HomeController> logger,
            IConfiguration config,
            IServicioPago servicioPago,
            IHtmlToPdfConverter pdfConverter,
            IServicioUsuarios servicioUsuarios,
            IServicioPropiedades servicioPropiedades,
            IPropiedadUsuarioService propiedadUsuario,
            IServicioGastoInmueble servicioGastoInmueble,
            IServicioContrato servicioContrato)
        {
            _logger = logger;
            _config = config;
            _servicioPago = servicioPago;
            _pdfConverter = pdfConverter;
            _servicioUsuarios = servicioUsuarios;
            _servicioPropiedades = servicioPropiedades;
            _propiedadUsuario = propiedadUsuario;
            _servicioGastoInmueble = servicioGastoInmueble;
            _servicioContrato = servicioContrato;
        }

        [Authorize(Policy = "UsuarioAutenticado")]
        public async Task<IActionResult> Index()
        {
            var pagos = new List<Pago>();
            var connectionString = _config.GetConnectionString("DefaultConnection");

            using var connection = new SqlConnection(connectionString);
            var query = "SELECT Monto_pago, Descripcion, Fecha_pago_programada, Archivo_factura FROM Pagos";

            using var command = new SqlCommand(query, connection);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                pagos.Add(new Pago
                {
                    Monto_pago = reader.GetDecimal(0),
                    Descripcion = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Fecha_pago_programada = reader.GetDateTime(2),
                    Archivo_factura = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }

            var eventos = pagos.Select(p => new
            {
                title = $"{p.Monto_pago}€ - {p.Descripcion}",
                start = p.Fecha_pago_programada.ToString("yyyy-MM-dd"),
                descripcion = p.Descripcion,
                monto = p.Monto_pago,
                fecha = p.Fecha_pago_programada.ToString("dd/MM/yyyy"),
                tipo = "Pago",
                archivo = p.Archivo_factura
            });

            ViewBag.EventosJson = JsonSerializer.Serialize(eventos);
            return View();
        }
        [Authorize(Policy = "UsuarioAutenticado")]
        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize(Policy = "UsuarioAutenticado")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        private bool EstaPagado(DateTime? fechaPagoReal) =>
            fechaPagoReal > new DateTime(1, 1, 1);

        [HttpGet]
        public async Task<IActionResult> ObtenerPagosCalendario()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Json(new List<object>());

            var eventos = new List<object>();

            var contratosComoInquilino = await _servicioPago.ObtenerContratosPorInquilinoAsync(userId);

            var contratosComoPropietario = await _servicioPago.ObtenerContratosPorPropietarioAsync(userId);

            foreach (var contrato in contratosComoInquilino)
            {
                var pagos = await _servicioPago.ObtenerPagosPorContratoAsync(contrato.Id_contrato);
                eventos.AddRange(pagos.Select(p => new
                {
                    title = $"{p.Monto_pago}€ - {p.Descripcion}",
                    start = p.Fecha_pago_programada.ToString("yyyy-MM-dd"),
                    color = EstaPagado(p.Fecha_pago_real) ? "#66b266" : "#ff6666",
                    descripcion = p.Descripcion,
                    monto = p.Monto_pago,
                    fecha = p.Fecha_pago_programada.ToString("dd/MM/yyyy"),
                    archivo = p.Archivo_factura,
                    tipo = "Debe pagar"
                }));
            }

            foreach (var contrato in contratosComoPropietario)
            {
                var pagos = await _servicioPago.ObtenerPagosPorContratoAsync(contrato.Id_contrato);
                eventos.AddRange(pagos.Select(p => new
                {
                    title = $"{p.Monto_pago}€ - {p.Descripcion}",
                    start = p.Fecha_pago_programada.ToString("yyyy-MM-dd"),
                    color = EstaPagado(p.Fecha_pago_real) ? "#66b266" : "#3366cc",
                    descripcion = p.Descripcion,
                    monto = p.Monto_pago,
                    fecha = p.Fecha_pago_programada.ToString("dd/MM/yyyy"),
                    archivo = p.Archivo_factura,
                    tipo = "Debe recibir"
                }));
            }

            return Json(eventos);
        }

        [Authorize]
        public IActionResult ExportarVista()
        {
            return View();
        }
        [Authorize(Policy = "EsAdministrador")]
        [HttpPost]
        public async Task<IActionResult> ExportarTodo(string formato, string entidad)
        {
            if (string.IsNullOrEmpty(formato))
                return BadRequest("Formato no especificado");
            if (string.IsNullOrEmpty(entidad))
                return BadRequest("Entidad no especificado");
            switch (entidad.ToLower())
            {
                case "usuarios":
                    {
                        var usuarios = await _servicioUsuarios.ObtenerTodos();
                        return GenerarArchivoUsuarios(usuarios, formato);
                    }
                case "propiedades":
                    {
                        var propiedades = await _servicioPropiedades.ObtenerTodas();
                        return await GenerarArchivoPropiedadesAsync(propiedades, formato);
                    }
                case "gastosinmueble":
                    {
                        var gastos = await _servicioGastoInmueble.ObtenerTodos();
                        return GenerarArchivoGastosInmueble(gastos, formato);
                    }
                default:
                    return BadRequest("Entidad no soportada");
            }
        }
        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ExportarFiltrado(DateTime? desde, DateTime? hasta, string formato, string entidad)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (string.IsNullOrEmpty(formato))
                return BadRequest("Formato no especificado");
            if (string.IsNullOrEmpty(entidad))
                return BadRequest("Entidad no especificado");

            bool sinFiltro = !desde.HasValue && !hasta.HasValue;

            switch (entidad.ToLower())
            {
                case "propiedades":
                    {
                        var propiedades = await _servicioPropiedades.ObtenerPropiedadesComoPropietarioAsync();

                        if (desde.HasValue)
                            propiedades = propiedades.Where(p => p.Fecha_adquisicion >= desde.Value).ToList();

                        if (hasta.HasValue)
                            propiedades = propiedades.Where(p => p.Fecha_adquisicion <= hasta.Value).ToList();

                        return await GenerarArchivoPropiedadesAsync(propiedades, formato);
                    }

                case "gastosinmueble":
                    {
                        var gastos = await ObtenerGastosUsuario(userId);

                        if (desde.HasValue)
                            gastos = gastos.Where(g => g.Fecha_pago >= desde.Value).ToList();

                        if (hasta.HasValue)
                            gastos = gastos.Where(g => g.Fecha_pago <= hasta.Value).ToList();

                        return GenerarArchivoGastosInmueble(gastos, formato);
                    }

                default:
                    return BadRequest("Entidad no soportada");
            }
        }
        private async Task<List<GastoInmueble>> ObtenerGastosUsuario(string userId)
        {
            var propiedadesUsuario = await _servicioPropiedades.ObtenerPropiedadesComoPropietarioAsync();
            var idsPropiedades = propiedadesUsuario.Select(p => p.Id_propiedad).ToList();
            return await _servicioGastoInmueble.ObtenerPorPropiedades(idsPropiedades);
        }

        private FileResult GenerarArchivoUsuarios(IEnumerable<Usuario> usuarios, string formato)
        {
            if (formato == "excel")
            {
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Usuarios");

                ws.Cell(1, 1).Value = "Nombre";
                ws.Cell(1, 2).Value = "Apellidos";
                ws.Cell(1, 3).Value = "NIF";
                ws.Cell(1, 4).Value = "Email";
                ws.Cell(1, 5).Value = "Contraseña";

                int row = 2;
                foreach (var u in usuarios)
                {
                    ws.Cell(row, 1).Value = u.Nombre;
                    ws.Cell(row, 2).Value = u.Apellidos;
                    ws.Cell(row, 3).Value = u.NIF;
                    ws.Cell(row, 4).Value = u.Email;
                    ws.Cell(row, 5).Value = u.Contraseña;
                    row++;
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Usuarios.xlsx");
            }
            else if (formato == "pdf")
            {
                var html = "<h1>Listado de Usuarios</h1><table border='1' cellpadding='5'>" +
                           "<tr><th>Nombre</th><th>Apellidos</th><th>NIF</th><th>Email</th><th>Contraseña</th></tr>";

                foreach (var u in usuarios)
                {
                    html += $"<tr><td>{u.Nombre}</td><td>{u.Apellidos}</td><td>{u.NIF}</td><td>{u.Email}</td><td>{u.Contraseña}</td></tr>";
                }

                html += "</table>";
                var pdfBytes = _pdfConverter.ConvertHtmlToPdf(html);
                return File(pdfBytes, "application/pdf", "Usuarios.pdf");
            }

            throw new InvalidOperationException("Formato no soportado");
        }
        private async Task<FileResult> GenerarArchivoPropiedadesAsync(IEnumerable<Propiedad> propiedades, string formato)
        {
            if (formato == "excel")
            {
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Propiedades");

                ws.Cell(1, 1).Value = "Dirección";
                ws.Cell(1, 2).Value = "Referencia catastral";
                ws.Cell(1, 3).Value = "Número de habitaciones";
                ws.Cell(1, 4).Value = "Propietarios";
                ws.Cell(1, 5).Value = "Fecha de adquisición";

                int row = 2;
                foreach (var prop in propiedades)
                {
                    ws.Cell(row, 1).Value = prop.Direccion;
                    ws.Cell(row, 2).Value = prop.Referencia_catastral ?? "";
                    ws.Cell(row, 3).Value = prop.numHabitaciones;

                    prop.Usuarios = await _propiedadUsuario.GetByPropiedadIdAsync(prop.Id_propiedad);
                    var propietarios = prop.Usuarios != null && prop.Usuarios.Any()
                        ? string.Join(", ", prop.Usuarios.Select(pu => $"{pu.Usuario.Nombre} ({pu.PorcentajePropiedad}%)"))
                        : "Sin propietario";

                    ws.Cell(row, 4).Value = propietarios;
                    ws.Cell(row, 5).Value = prop.Fecha_adquisicion.ToString("dd/MM/yyyy");
                    row++;
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Propiedades.xlsx");
            }
            else if (formato == "pdf")
            {
                var html = "<h1>Listado de Propiedades</h1><table border='1' cellpadding='5'>" +
                           "<tr><th>Dirección</th><th>Referencia catastral</th><th>Habitaciones</th><th>Propietarios</th><th>Fecha de adquisición</th></tr>";

                foreach (var prop in propiedades)
                {
                    prop.Usuarios = await _propiedadUsuario.GetByPropiedadIdAsync(prop.Id_propiedad);
                    var propietarios = prop.Usuarios != null && prop.Usuarios.Any()
                        ? string.Join(", ", prop.Usuarios.Select(pu => $"{pu.Usuario.Nombre} ({pu.PorcentajePropiedad}%)"))
                        : "Sin propietario";

                    html += $"<tr><td>{prop.Direccion}</td><td>{prop.Referencia_catastral}</td><td>{prop.numHabitaciones}</td><td>{propietarios}</td><td>{prop.Fecha_adquisicion}</td></tr>";
                }

                html += "</table>";
                var pdfBytes = _pdfConverter.ConvertHtmlToPdf(html);
                return File(pdfBytes, "application/pdf", "Propiedades.pdf");
            }

            throw new InvalidOperationException("Formato no soportado");
        }
        private FileResult GenerarArchivoGastosInmueble(IEnumerable<GastoInmueble> gastos, string formato)
        {
            if (formato == "excel")
            {
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Gastos Inmueble");

                ws.Cell(1, 1).Value = "Tipo de Gasto";
                ws.Cell(1, 2).Value = "Monto (€)";
                ws.Cell(1, 3).Value = "Fecha de Pago";
                ws.Cell(1, 4).Value = "Porcentaje Amortización (%)";
                ws.Cell(1, 5).Value = "Repercutible";
                ws.Cell(1, 6).Value = "Descripción";
                ws.Cell(1, 7).Value = "Propiedad (Dirección)";

                int row = 2;
                foreach (var g in gastos)
                {
                    ws.Cell(row, 1).Value = g.Tipo_gasto;
                    ws.Cell(row, 2).Value = g.Monto_gasto;
                    ws.Cell(row, 3).Value = g.Fecha_pago.ToString("dd/MM/yyyy");
                    ws.Cell(row, 4).Value = g.Porcentaje_amortizacion;
                    ws.Cell(row, 5).Value = g.Repercutible ? "Sí" : "No";
                    ws.Cell(row, 6).Value = g.Descripcion ?? "";
                    ws.Cell(row, 7).Value = g.Propiedad?.Direccion ?? "N/A";
                    row++;
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "GastosInmueble.xlsx");
            }
            else if (formato == "pdf")
            {
                var html = "<h1>Listado de Gastos Inmueble</h1><table border='1' cellpadding='5'>" +
                           "<tr><th>Tipo de Gasto</th><th>Monto</th><th>Fecha de Pago</th><th>Amortización (%)</th><th>Repercutible</th><th>Descripción</th><th>Propiedad</th></tr>";

                foreach (var g in gastos)
                {
                    html += $"<tr><td>{g.Tipo_gasto}</td><td>{g.Monto_gasto}€</td><td>{g.Fecha_pago:dd/MM/yyyy}</td><td>{g.Porcentaje_amortizacion}</td><td>{(g.Repercutible ? "Sí" : "No")}</td><td>{g.Descripcion}</td><td>{g.Propiedad?.Direccion ?? "N/A"}</td></tr>";
                }

                html += "</table>";
                var pdfBytes = _pdfConverter.ConvertHtmlToPdf(html);
                return File(pdfBytes, "application/pdf", "GastosInmueble.pdf");
            }

            throw new InvalidOperationException("Formato no soportado");
        }
    }
}

