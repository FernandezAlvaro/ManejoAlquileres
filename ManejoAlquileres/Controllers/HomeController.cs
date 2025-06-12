using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using ManejoAlquileres.Models;
using ManejoAlquileres.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ManejoAlquileres.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;
        private readonly IServicioPago _servicioPago;

        public HomeController(ILogger<HomeController> logger, IConfiguration config, IServicioPago servicioPago)
        {
            _logger = logger;
            _config = config;
            _servicioPago = servicioPago;
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
        public async Task<IActionResult> DescargarFactura(string nombreArchivo)
        {
            var path = Path.Combine("wwwroot", "facturas", nombreArchivo);
            if (!System.IO.File.Exists(path))
                return NotFound();

            var bytes = await System.IO.File.ReadAllBytesAsync(path);
            return File(bytes, "application/pdf", nombreArchivo);
        }
    }
}