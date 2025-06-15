using System.Data;
using ManejoAlquileres.Models;
using ManejoAlquileres.Models.DTO;
using ManejoAlquileres.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace ManejoAlquileres.Service
{
    public class ServicioPago : IServicioPago
    {
        private readonly ApplicationDbContext _context;

        public ServicioPago(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Crear(Pago pago)
        {
            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();
        }

        public async Task<Pago> ObtenerPorId(string id)
        {
            return await _context.Pagos
                .Include(p => p.Contrato)
                    .ThenInclude(c => c.Propiedad)
                .Include(p => p.Contrato)
                    .ThenInclude(c => c.Inquilinos)
                .Include(p => p.Contrato)
                    .ThenInclude(c => c.Propietarios)
                .FirstOrDefaultAsync(p => p.Id_pago == id);
        }

        public async Task<IEnumerable<Pago>> ObtenerTodos()
        {
            return await _context.Pagos
                .Include(p => p.Contrato)
                .ToListAsync();
        }

        public async Task Actualizar(Pago pago)
        {
            _context.Pagos.Update(pago);
            await _context.SaveChangesAsync();
        }

        public async Task Borrar(string id)
        {
            var pago = await _context.Pagos.FindAsync(id);
            if (pago != null)
            {
                _context.Pagos.Remove(pago);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> Existe(string id)
        {
            return await _context.Pagos.AnyAsync(p => p.Id_pago == id);
        }

        public async Task<List<Pago>> ObtenerPagosARealizarPorUsuarioAsync(string idUsuario)
        {
            return await _context.Pagos
                .Where(p => p.Contrato.Propietarios.Any(cp => cp.UsuarioId == idUsuario))
                .ToListAsync();
        }

        public async Task<List<Pago>> ObtenerPagosARecibirPorUsuarioAsync(string idUsuario)
        {
            return await _context.Pagos
                .Where(p => p.Contrato.Propietarios.Any(cp => cp.UsuarioId == idUsuario))
                .ToListAsync();
        }
        public async Task<List<PagoConContratoDTO>> ObtenerPagosConDatosContrato()
        {
            return await _context.Pagos
                .Include(p => p.Contrato)
                    .ThenInclude(c => c.Inquilinos)
                .Include(p => p.Contrato)
                    .ThenInclude(c => c.Propietarios)
                .Include(p => p.Contrato)
                    .ThenInclude(c => c.Propiedad)
                .Select(p => new PagoConContratoDTO
                {
                    Id_pago = p.Id_pago,
                    Fecha_pago_programada = p.Fecha_pago_programada,
                    Fecha_pago_real = p.Fecha_pago_real,
                    Monto_pago = p.Monto_pago,
                    Descripcion = p.Descripcion,
                    Archivo_factura = p.Archivo_factura,

                    Id_contrato = p.Contrato.Id_contrato,
                    Direccion_propiedad = p.Contrato.Propiedad.Direccion,
                    Fecha_inicio_contrato = p.Contrato.Fecha_inicio,
                    Fecha_fin_contrato = p.Contrato.Fecha_fin,

                    Id_inquilinos = p.Contrato.Inquilinos.Select(i => i.UsuarioId).ToList(),
                    Id_duenios = p.Contrato.Propietarios.Select(d => d.UsuarioId).ToList()
                })
                .ToListAsync();
        }

        public async Task<List<Contrato>> ObtenerContratosPorInquilinoAsync(string usuarioId)
        {
            return await _context.Contratos
                .Include(c => c.Propiedad)
                .Include(c => c.Inquilinos)
                .Where(c => c.Inquilinos.Any(i => i.UsuarioId == usuarioId))
                .ToListAsync();
        }

        public async Task<List<Contrato>> ObtenerContratosPorPropietarioAsync(string usuarioId)
        {
            return await _context.Contratos
                .Include(c => c.Propiedad)
                .Include(c => c.Propietarios)
                .Where(c => c.Propietarios.Any(p => p.UsuarioId == usuarioId))
                .ToListAsync();
        }

        public async Task<List<Pago>> ObtenerPagosPorContratoAsync(string contratoId)
        {
            return await _context.Pagos
                .Where(p => p.Id_contrato== contratoId)
                .ToListAsync();
        }
    }
}