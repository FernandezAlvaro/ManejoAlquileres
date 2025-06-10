using ManejoAlquileres.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManejoAlquileres.Controllers
{
    [ApiController]
    [Route("api/habitaciones")]
    public class HabitacionesApiController : ControllerBase
    {
        private readonly IServicioHabitacion _servicio;
        private readonly ApplicationDbContext _context;

        public HabitacionesApiController(IServicioHabitacion servicio, ApplicationDbContext context)
        {
            _servicio = servicio;
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetHabitacionesPorPropiedad(string id)
        {
            var habitaciones = await _context.Habitaciones
                .Where(h => h.Id_propiedad == id)
                .Select(h => new {
                    id_habitacion = h.Id_habitacion,
                    descripcion = h.Descripcion
                }).ToListAsync();

            return Ok(habitaciones);
        }
    }
}
