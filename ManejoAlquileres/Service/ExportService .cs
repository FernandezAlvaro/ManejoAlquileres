using ClosedXML.Excel;
using ManejoAlquileres.Models;
using ManejoAlquileres.Models.Helpers;
using ManejoAlquileres.Service.Interface;

namespace ManejoAlquileres.Service
{
    public class ExportService : IExportService
    {
        private readonly ApplicationDbContext _context;

        public ExportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public ArchivoExportado ExportarPagosFiltrados(DateTime? desde, DateTime? hasta, string estado, string tipo, string formato, string userId)
        {
            var contratos = new List<Contrato>();

            if (string.IsNullOrEmpty(tipo) || tipo == "pagar")
            {
                contratos.AddRange(_context.Contratos
                    .Where(c => c.Inquilinos.Any(i => i.UsuarioId == userId))
                    .ToList());
            }

            if (string.IsNullOrEmpty(tipo) || tipo == "recibir")
            {
                contratos.AddRange(_context.Contratos
                    .Where(c => c.Propietarios.Any(p => p.UsuarioId == userId))
                    .ToList());
            }

            var pagos = contratos.SelectMany(c => c.Pagos).ToList();

            if (desde.HasValue)
                pagos = pagos.Where(p => p.Fecha_pago_programada >= desde.Value).ToList();

            if (hasta.HasValue)
                pagos = pagos.Where(p => p.Fecha_pago_programada <= hasta.Value).ToList();

            if (estado == "pagado")
                pagos = pagos.Where(p => p.Fecha_pago_real.HasValue).ToList();
            else if (estado == "pendiente")
                pagos = pagos.Where(p => !p.Fecha_pago_real.HasValue).ToList();

            if (formato == "excel")
            {
                using var workbook = new XLWorkbook();
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
                return new ArchivoExportado
                {
                    Contenido = stream.ToArray(),
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    NombreArchivo = "PagosFiltrados.xlsx"
                };
            }

            throw new NotSupportedException("Formato no soportado para exportar pagos.");
        }

        public ArchivoExportado ExportarPropiedadesUsuario(string userId, string formato)
        {
            var propiedadesUsuario = _context.PropiedadesUsuarios
                .Where(pu => pu.UsuarioId == userId)
                .Select(pu => new
                {
                    pu.Propiedad.Id_propiedad,
                    pu.Propiedad.Direccion,
                    pu.PorcentajePropiedad,
                    pu.Propiedad.Valor_adquisicion,
                    pu.Propiedad.Valor_catastral_piso,
                    pu.Propiedad.Valor_catastral_terreno
                }).ToList();

            if (formato == "excel")
            {
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Propiedades");

                ws.Cell(1, 1).Value = "ID Propiedad";
                ws.Cell(1, 2).Value = "Dirección";
                ws.Cell(1, 3).Value = "Porcentaje Propiedad";
                ws.Cell(1, 4).Value = "Valor Adquisición";
                ws.Cell(1, 5).Value = "Valor Catastral Piso";
                ws.Cell(1, 6).Value = "Valor Catastral Terreno";

                int row = 2;
                foreach (var p in propiedadesUsuario)
                {
                    ws.Cell(row, 1).Value = p.Id_propiedad;
                    ws.Cell(row, 2).Value = p.Direccion;
                    ws.Cell(row, 3).Value = p.PorcentajePropiedad;
                    ws.Cell(row, 4).Value = p.Valor_adquisicion;
                    ws.Cell(row, 5).Value = p.Valor_catastral_piso;
                    ws.Cell(row, 6).Value = p.Valor_catastral_terreno;
                    row++;
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return new ArchivoExportado
                {
                    Contenido = stream.ToArray(),
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    NombreArchivo = "PropiedadesUsuario.xlsx"
                };
            }

            throw new NotSupportedException("Formato no soportado para exportar propiedades.");
        }

        public ArchivoExportado ExportarTodaBaseDeDatos(string formato)
        {
            // Ejemplo: Exportar todas las tablas importantes en un Excel con pestañas
            if (formato == "excel")
            {
                using var workbook = new XLWorkbook();

                // Usuarios
                var usuarios = _context.Usuarios.ToList();
                var wsUsuarios = workbook.Worksheets.Add("Usuarios");
                wsUsuarios.Cell(1, 1).Value = "ID Usuario";
                wsUsuarios.Cell(1, 2).Value = "Nombre";
                wsUsuarios.Cell(1, 3).Value = "Apellidos";
                wsUsuarios.Cell(1, 4).Value = "Email";
                wsUsuarios.Cell(1, 5).Value = "Teléfono";
                wsUsuarios.Cell(1, 6).Value = "Es Administrador";

                int rowU = 2;
                foreach (var u in usuarios)
                {
                    wsUsuarios.Cell(rowU, 1).Value = u.Id_usuario;
                    wsUsuarios.Cell(rowU, 2).Value = u.Nombre;
                    wsUsuarios.Cell(rowU, 3).Value = u.Apellidos;
                    wsUsuarios.Cell(rowU, 4).Value = u.Email;
                    wsUsuarios.Cell(rowU, 5).Value = u.Telefono;
                    wsUsuarios.Cell(rowU, 6).Value = u.EsAdministrador;
                    rowU++;
                }

                // Propiedades
                var propiedades = _context.Propiedades.ToList();
                var wsPropiedades = workbook.Worksheets.Add("Propiedades");
                wsPropiedades.Cell(1, 1).Value = "ID Propiedad";
                wsPropiedades.Cell(1, 2).Value = "Dirección";
                wsPropiedades.Cell(1, 3).Value = "Valor Adquisición";

                int rowP = 2;
                foreach (var p in propiedades)
                {
                    wsPropiedades.Cell(rowP, 1).Value = p.Id_propiedad;
                    wsPropiedades.Cell(rowP, 2).Value = p.Direccion;
                    wsPropiedades.Cell(rowP, 3).Value = p.Valor_adquisicion;
                    rowP++;
                }

                // Agrega más hojas para otras entidades si quieres

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return new ArchivoExportado
                {
                    Contenido = stream.ToArray(),
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    NombreArchivo = "BaseDeDatosCompleta.xlsx"
                };
            }

            throw new NotSupportedException("Formato no soportado para exportar toda la base de datos.");
        }
    }
}