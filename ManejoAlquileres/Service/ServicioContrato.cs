﻿using ManejoAlquileres.Models;
using ManejoAlquileres.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace ManejoAlquileres.Service
{
    public class ServicioContrato : IServicioContrato
    {
        private readonly ApplicationDbContext _context;

        public ServicioContrato(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Crear(Contrato contrato)
        {
            _context.Contratos.Add(contrato);
            await _context.SaveChangesAsync();
        }

        public async Task<Contrato> ObtenerPorId(string id)
        {
            return await _context.Contratos
                .Include(c => c.Propiedad)
                .Include(c => c.Habitacion)
                .Include(c => c.Inquilinos).ThenInclude(ci => ci.Usuario)
                .Include(c => c.Propietarios).ThenInclude(cp => cp.Usuario)
                .Include(c => c.Pagos)
                .FirstOrDefaultAsync(c => c.Id_contrato == id);
        }

        public async Task<List<Contrato>> ObtenerTodos()
        {
            return await _context.Contratos
                .Include(c => c.Propiedad)
                .Include(c => c.Habitacion)
                .ToListAsync();
        }

        public async Task Actualizar(Contrato contrato)
        {
            _context.Contratos.Update(contrato);
            await _context.SaveChangesAsync();
        }

        public async Task Borrar(string id)
        {
            var contrato = await _context.Contratos.FindAsync(id);
            if (contrato != null)
            {
                _context.Contratos.Remove(contrato);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> Existe(string id)
        {
            return await _context.Contratos.AnyAsync(c => c.Id_contrato == id);
        }

        public async Task<List<Contrato>> ObtenerContratosDelUsuario(string userId)
        {
            var contratosInquilino = await _context.ContratosInquilinos
                .Where(ci => ci.UsuarioId == userId)
                .Include(ci => ci.Contrato)
                    .ThenInclude(c => c.Pagos)
                .Include(ci => ci.Contrato.Propietarios).ThenInclude(cp => cp.Usuario)
                .Include(ci => ci.Contrato.Inquilinos).ThenInclude(ci => ci.Usuario)
                .Include(ci => ci.Contrato.Propiedad)
                .Include(ci => ci.Contrato.Habitacion)
                .Select(ci => ci.Contrato)
                .ToListAsync();

            var contratosPropietario = await _context.ContratosPropietarios
                .Where(cp => cp.UsuarioId == userId)
                .Include(cp => cp.Contrato)
                    .ThenInclude(c => c.Pagos)
                .Include(cp => cp.Contrato.Propietarios).ThenInclude(cp => cp.Usuario)
                .Include(cp => cp.Contrato.Inquilinos).ThenInclude(ci => ci.Usuario)
                .Include(cp => cp.Contrato.Propiedad)
                .Include(cp => cp.Contrato.Habitacion)
                .Select(cp => cp.Contrato)
                .ToListAsync();

            return contratosInquilino.Concat(contratosPropietario).Distinct().ToList();
        }
        public async Task<Contrato> Eliminar(string id)
        {
            var contrato = await _context.Contratos
                .Include(c => c.Propiedad)
                .Include(c => c.Habitacion)
                .Include(c => c.Inquilinos).ThenInclude(ci => ci.Usuario)
                .Include(c => c.Propietarios).ThenInclude(cp => cp.Usuario)
                .Include(c => c.Pagos)
                .FirstOrDefaultAsync(c => c.Id_contrato == id);

            if (contrato != null)
            {
                _context.Contratos.Remove(contrato);
                await _context.SaveChangesAsync();
            }

            return contrato;
        }

    }
}