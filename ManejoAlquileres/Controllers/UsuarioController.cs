using DocumentFormat.OpenXml.Spreadsheet;
using ManejoAlquileres.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManejoAlquileres.Controllers
{
    [Authorize]
    public class UsuarioController : Controller
    {
        private readonly IServicioUsuarios _servicioUsuarios;

        public UsuarioController(IServicioUsuarios servicioUsuarios)
        {
            _servicioUsuarios = servicioUsuarios;
        }

        public async Task<IActionResult> Perfil()
        {
            var idUsuario = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(idUsuario))
            {
                return Unauthorized();
            }

            var usuario = await _servicioUsuarios.ObtenerPorId(idUsuario);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }
    }
}