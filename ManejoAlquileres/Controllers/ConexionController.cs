using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ManejoAlquileres.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConexionController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public ConexionController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpGet("probar")]
        public async Task<IActionResult> ProbarConexion()
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                return Ok("Conexión exitosa a la base de datos.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al conectar: {ex.Message}");
            }
        }
    }
}