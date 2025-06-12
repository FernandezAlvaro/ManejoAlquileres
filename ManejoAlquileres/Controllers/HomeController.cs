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

        public HomeController(
            ILogger<HomeController> logger,
            IConfiguration config,
            IServicioPago servicioPago,
            IHtmlToPdfConverter pdfConverter,
            IServicioUsuarios servicioUsuarios,
            IServicioPropiedades servicioPropiedades)
        {
            _logger = logger;
            _config = config;
            _servicioPago = servicioPago;
            _pdfConverter = pdfConverter;
            _servicioUsuarios = servicioUsuarios;
            _servicioPropiedades = servicioPropiedades;
        }

        [Authorize(Policy = "UsuarioAutenticado")]
        public async Task<IActionResult> Index()
        {
            var pagos = new List<Pago>();
            var connectionString = _config.GetConnectionString("DefaultConnection");

            using var connection = new SqlConnection(connectionString);
            var query = "SELECT Monto_pago, Descripcion, Fecha_pago_programada FROM Pagos";

            using var command = new SqlCommand(query, connection);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                pagos.Add(new Pago
                {
                    Monto_pago = reader.GetDecimal(0),
                    Descripcion = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Fecha_pago_programada = reader.GetDateTime(2)
                });
            }

            var eventos = pagos.Select(p => new
            {
                title = $"{p.Monto_pago}€ - {p.Descripcion}",
                start = p.Fecha_pago_programada.ToString("yyyy-MM-dd")
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

        // Método auxiliar para verificar si el pago está realizado
        private bool EstaPagado(DateTime? fechaPagoReal) =>
            fechaPagoReal > new DateTime(1, 1, 1);

        [HttpGet]
        public async Task<IActionResult> ObtenerPagosCalendario()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Json(new List<object>());

            var eventos = new List<object>();

            // Obtener contratos donde es inquilino
            var contratosComoInquilino = await _servicioPago.ObtenerContratosPorInquilinoAsync(userId);

            // Obtener contratos donde es propietario
            var contratosComoPropietario = await _servicioPago.ObtenerContratosPorPropietarioAsync(userId);

            // Obtener pagos para contratos donde es inquilino (debe pagar)
            foreach (var contrato in contratosComoInquilino)
            {
                var pagos = await _servicioPago.ObtenerPagosPorContratoAsync(contrato.Id_contrato);
                eventos.AddRange(pagos.Select(p => new
                {
                    title = $"{p.Monto_pago}€ - {p.Descripcion}",
                    start = p.Fecha_pago_programada.ToString("yyyy-MM-dd"),
                    color = EstaPagado(p.Fecha_pago_real) ? "#66b266" : "#ff6666", // verde si pagado, rojo si pendiente
                    descripcion = p.Descripcion,
                    monto = p.Monto_pago,
                    fecha = p.Fecha_pago_programada.ToString("dd/MM/yyyy"),
                    archivo = p.Archivo_factura,
                    tipo = "Debe pagar"
                }));
            }

            // Obtener pagos para contratos donde es propietario (debe recibir)
            foreach (var contrato in contratosComoPropietario)
            {
                var pagos = await _servicioPago.ObtenerPagosPorContratoAsync(contrato.Id_contrato);
                eventos.AddRange(pagos.Select(p => new
                {
                    title = $"{p.Monto_pago}€ - {p.Descripcion}",
                    start = p.Fecha_pago_programada.ToString("yyyy-MM-dd"),
                    color = EstaPagado(p.Fecha_pago_real) ? "#66b266" : "#3366cc", // verde si pagado, azul si pendiente
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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ExportarFiltrado(DateTime? desde, DateTime? hasta, string estado, string tipo, string formato, string entidad)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            entidad ??= "pagos";

            if (entidad == "pagos")
            {
                var pagos = new List<Pago>();
                var contratos = new List<Contrato>();

                if (string.IsNullOrEmpty(tipo) || tipo == "pagar")
                    contratos.AddRange(await _servicioPago.ObtenerContratosPorInquilinoAsync(userId));

                if (string.IsNullOrEmpty(tipo) || tipo == "recibir")
                    contratos.AddRange(await _servicioPago.ObtenerContratosPorPropietarioAsync(userId));

                foreach (var contrato in contratos)
                    pagos.AddRange(await _servicioPago.ObtenerPagosPorContratoAsync(contrato.Id_contrato));

                // Aplicar filtros
                if (desde.HasValue)
                    pagos = pagos.Where(p => p.Fecha_pago_programada >= desde.Value).ToList();

                if (hasta.HasValue)
                    pagos = pagos.Where(p => p.Fecha_pago_programada <= hasta.Value).ToList();

                if (estado == "pagado")
                    pagos = pagos.Where(p => p.Fecha_pago_real.HasValue).ToList();
                else if (estado == "pendiente")
                    pagos = pagos.Where(p => !p.Fecha_pago_real.HasValue).ToList();

                // Exportar
                if (formato == "excel")
                {
                    using var workbook = new ClosedXML.Excel.XLWorkbook();
                    var ws = workbook.Worksheets.Add("Pagos");

                    ws.Cell(1, 1).Value = "Descripción";
                    ws.Cell(1, 2).Value = "Monto (€)";
                    ws.Cell(1, 3).Value = "Fecha Programada";
                    ws.Cell(1, 4).Value = "Fecha Real";
                    ws.Cell(1, 5).Value = "Estado";

                    int row = 2;
                    foreach (var p in pagos)
                    {
                        ws.Cell(row, 1).Value = p.Descripcion ?? "Sin descripción";
                        ws.Cell(row, 2).Value = p.Monto_pago;
                        ws.Cell(row, 3).Value = p.Fecha_pago_programada.ToString("dd/MM/yyyy");
                        ws.Cell(row, 4).Value = p.Fecha_pago_real?.ToString("dd/MM/yyyy") ?? "Pendiente";
                        ws.Cell(row, 5).Value = p.Fecha_pago_real.HasValue ? "Pagado" : "Pendiente";
                        row++;
                    }

                    using var stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "PagosFiltrados.xlsx");
                }
                else if (formato == "pdf")
                {
                    var html = "<h1>Resumen de Pagos</h1><table border='1' cellpadding='5'>" +
                               "<tr><th>Descripción</th><th>Monto</th><th>Fecha Programada</th><th>Fecha Real</th><th>Estado</th></tr>";

                    foreach (var p in pagos)
                    {
                        html += $"<tr><td>{p.Descripcion ?? "Sin descripción"}</td><td>{p.Monto_pago}€</td><td>{p.Fecha_pago_programada:dd/MM/yyyy}</td><td>{(p.Fecha_pago_real?.ToString("dd/MM/yyyy") ?? "Pendiente")}</td><td>{(p.Fecha_pago_real.HasValue ? "Pagado" : "Pendiente")}</td></tr>";
                    }

                    html += "</table>";

                    var pdfBytes = _pdfConverter.ConvertHtmlToPdf(html);
                    return File(pdfBytes, "application/pdf", "PagosFiltrados.pdf");
                }

                return BadRequest("Formato no soportado");
            }

            // --------------------------------------
            // PROPIEDADES
            // --------------------------------------
            else if (entidad == "propiedades")
            {
                var propiedades = await _servicioPropiedades.ObtenerPropiedadesComoPropietarioAsync();

                // Filtros opcionales
                if (desde.HasValue)
                    propiedades = propiedades.Where(p => p.Fecha_adquisicion >= desde.Value).ToList();

                if (hasta.HasValue)
                    propiedades = propiedades.Where(p => p.Fecha_adquisicion <= hasta.Value).ToList();

                if (estado == "activo")
                    propiedades = propiedades.Where(p => p.Estado_propiedad).ToList();
                else if (estado == "inactivo")
                    propiedades = propiedades.Where(p => !p.Estado_propiedad).ToList();

                if (formato == "excel")
                {
                    using var workbook = new ClosedXML.Excel.XLWorkbook();
                    var ws = workbook.Worksheets.Add("Propiedades");

                    ws.Cell(1, 1).Value = "Dirección";
                    ws.Cell(1, 2).Value = "Referencia catastral";
                    ws.Cell(1, 3).Value = "Número de habitaciones";
                    ws.Cell(1, 4).Value = "Propietarios";

                    int row = 2;
                    foreach (var prop in propiedades)
                    {
                        ws.Cell(row, 1).Value = prop.Direccion;
                        ws.Cell(row, 2).Value = prop.Referencia_catastral ?? "";
                        ws.Cell(row, 3).Value = prop.numHabitaciones;

                        var propietarios = prop.Usuarios != null && prop.Usuarios.Any()
                            ? string.Join(", ", prop.Usuarios.Select(pu => $"{pu.Usuario.Nombre} ({pu.PorcentajePropiedad}%)"))
                            : "Sin propietario";

                        ws.Cell(row, 4).Value = propietarios;
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
                               "<tr><th>Dirección</th><th>Referencia catastral</th><th>Número de habitaciones</th><th>Propietarios</th></tr>";

                    foreach (var prop in propiedades)
                    {
                        var propietarios = prop.Usuarios != null && prop.Usuarios.Any()
                            ? string.Join(", ", prop.Usuarios.Select(pu => $"{pu.Usuario.Nombre} ({pu.PorcentajePropiedad}%)"))
                            : "Sin propietario";

                        html += $"<tr><td>{prop.Direccion}</td><td>{prop.Referencia_catastral}</td><td>{prop.numHabitaciones}</td><td>{propietarios}</td></tr>";
                    }

                    html += "</table>";

                    var pdfBytes = _pdfConverter.ConvertHtmlToPdf(html);
                    return File(pdfBytes, "application/pdf", "Propiedades.pdf");
                }

                return BadRequest("Formato no soportado");
            }
            else if (entidad == "usuarios")
            {
                var usuarios = await _servicioUsuarios.ObtenerTodos(); // O el método que uses

                if (formato == "excel")
                {
                    using var workbook = new ClosedXML.Excel.XLWorkbook();
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
                               "<tr><th>Nombre</th><th>Email</th><th>Rol</th></tr>";

                    foreach (var u in usuarios)
                    {
                        html += $"<tr><td>{u.Nombre}</td><td>{u.Apellidos}</td><td>{u.NIF}</td><td>{u.Email}</td><td>{u.Contraseña}</td></tr>";
                    }

                    html += "</table>";

                    var pdfBytes = _pdfConverter.ConvertHtmlToPdf(html);
                    return File(pdfBytes, "application/pdf", "Usuarios.pdf");
                }

                return BadRequest("Formato no soportado");
            }

            return BadRequest("Entidad no soportada");
        }
    }
}