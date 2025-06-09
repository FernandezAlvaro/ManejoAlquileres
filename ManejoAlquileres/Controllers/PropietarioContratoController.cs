//using ManejoAlquileres.Models;
//using ManejoAlquileres.Service;
//using Microsoft.AspNetCore.Mvc;

//namespace ManejoAlquileres.Controllers
//{
//[Authorize(Policy = "UsuarioAutenticado")]
//    [ApiController]
//    [Route("api/[controller]")]
//    public class PropietarioContratoController : ControllerBase
//    {
//        private readonly IServicioPropietarioContrato servicioPropietarioContrato;

//        public PropietarioContratoController(IServicioPropietarioContrato servicioPropietarioContrato)
//        {
//            this.servicioPropietarioContrato = servicioPropietarioContrato;
//        }

//        // POST: api/PropietarioContrato/asignar
//        [HttpPost("asignar")]
//        public async Task<IActionResult> AsignarPropietarioAContrato([FromBody] PropietarioContrato relacion)
//        {
//            if (relacion == null || string.IsNullOrEmpty(relacion.Id_contrato) || string.IsNullOrEmpty(relacion.Id_propietario))
//                return BadRequest("Datos inválidos");

//            await servicioPropietarioContrato.Crear(relacion);
//            return Ok("Relación creada con éxito");
//        }

//        // GET: api/PropietarioContrato/por-contrato/C12345678
//        [HttpGet("por-contrato/{idContrato}")]
//        public async Task<IActionResult> ObtenerPropietariosPorContrato(string idContrato)
//        {
//            var propietarios = await servicioPropietarioContrato.ObtenerDetallePropietariosPorContrato(idContrato);
//            return Ok(propietarios);
//        }

//        // DELETE: api/PropietarioContrato/eliminar?contrato=C123&propietario=U456
//        [HttpDelete("eliminar")]
//        public async Task<IActionResult> EliminarRelacion(string contrato, string propietario)
//        {
//            await servicioPropietarioContrato.Eliminar(contrato, propietario);
//            return Ok("Relación eliminada");
//        }
//    }
//}