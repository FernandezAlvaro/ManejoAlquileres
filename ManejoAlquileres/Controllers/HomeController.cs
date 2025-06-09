using System.Diagnostics;
using System.Text.Json;
using ManejoAlquileres.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ManejoAlquileres.Controllers
{
    [Authorize(Policy = "UsuarioAutenticado")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
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
                    Descripcion = reader.GetString(1),
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
    }
}
