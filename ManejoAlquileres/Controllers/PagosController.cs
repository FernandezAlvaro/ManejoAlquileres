using System.Security.Claims;
using ManejoAlquileres.Models;
using ManejoAlquileres.Models.DTO;
using ManejoAlquileres.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManejoAlquileres.Controllers
{
    [Authorize]
    public class PagosController : Controller
    {
        private readonly IServicioPago _servicioPago;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PagosController(IServicioPago servicioPago, IWebHostEnvironment webHostEnvironment)
        {
            _servicioPago = servicioPago;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string orden)
        {
            var usuarioActualId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioActualId))
                return Unauthorized();

            var pagos = await _servicioPago.ObtenerPagosConDatosContrato();

            var pagosComoInquilino = pagos
                .Where(p => p.Id_inquilino == usuarioActualId)
                .OrderBy(p => p.Direccion_propiedad)
                .ToList();

            var pagosComoPropietario = pagos
                .Where(p => p.Id_duenio == usuarioActualId)
                .OrderBy(p => p.Direccion_propiedad)
                .ToList();

            var modelo = new PagosSeparadosDTO
            {
                PagosComoInquilino = pagosComoInquilino,
                PagosComoPropietario = pagosComoPropietario
            };

            return View(modelo);
        }

        public async Task<IActionResult> Editar(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("ID inválido.");

            var pago = await _servicioPago.ObtenerPorId(id);
            if (pago == null)
                return NotFound();

            return View(pago);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(string id, [Bind("Id_pago,Fecha_pago_real,Descripcion")] Pago pagoEdit, IFormFile archivoFacturaArchivo)
        {
            if (id != pagoEdit.Id_pago)
                return NotFound();


            var pagoOriginal = await _servicioPago.ObtenerPorId(id);
            if (pagoOriginal == null)
                return NotFound();
            ModelState.Remove("Id_contrato");
            if (!ModelState.IsValid)
            {
                pagoEdit.Fecha_pago_programada = pagoOriginal.Fecha_pago_programada;
                pagoEdit.Archivo_factura = pagoOriginal.Archivo_factura;

                return View(pagoEdit);
            }

            try
            {
                pagoOriginal.Fecha_pago_real = pagoEdit.Fecha_pago_real;
                pagoOriginal.Descripcion = pagoEdit.Descripcion;
                if (archivoFacturaArchivo != null && archivoFacturaArchivo.Length > 0)
                {
                    var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "facturas");
                    Directory.CreateDirectory(uploadsPath);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(archivoFacturaArchivo.FileName);
                    var filePath = Path.Combine(uploadsPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await archivoFacturaArchivo.CopyToAsync(stream);
                    }

                    pagoOriginal.Archivo_factura = Path.Combine("facturas", fileName).Replace("\\", "/");
                }

                await _servicioPago.Actualizar(pagoOriginal);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Ocurrió un error al guardar los cambios.");
                pagoEdit.Fecha_pago_programada = pagoOriginal.Fecha_pago_programada;
                pagoEdit.Archivo_factura = pagoOriginal.Archivo_factura;

                return View(pagoEdit);
            }
        }
        public async Task<IActionResult> Ver(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("ID inválido.");

            var pago = await _servicioPago.ObtenerPorId(id);

            if (pago == null || pago.Contrato == null || pago.Contrato.Propiedad == null)
                return NotFound();

            var dto = new PagoConContratoDTO
            {
                Id_pago = pago.Id_pago,
                Fecha_pago_programada = pago.Fecha_pago_programada,
                Fecha_pago_real = pago.Fecha_pago_real,
                Monto_pago = pago.Monto_pago,
                Descripcion = pago.Descripcion,
                Archivo_factura = pago.Archivo_factura,
                Direccion_propiedad = pago.Contrato.Propiedad.Direccion
            };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Pagar(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("ID inválido.");

            var pago = await _servicioPago.ObtenerPorId(id);
            if (pago == null)
                return NotFound();
            if (pago.Fecha_pago_real != new DateTime(1, 1, 1))
                return BadRequest("El pago ya fue realizado.");

            pago.Fecha_pago_real = DateTime.Now;

            await _servicioPago.Actualizar(pago);

            return RedirectToAction(nameof(Index));
        }


    }
}