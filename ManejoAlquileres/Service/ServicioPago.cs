using System.Data;
using Dapper;
using ManejoAlquileres.Models;
using ManejoAlquileres.Service.Interface;
using Microsoft.Data.SqlClient;

namespace ManejoAlquileres.Service
{
    public class ServicioPago:IServicioPago
    {
        private readonly string connectionString;

        public ServicioPago(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task Crear(Pago pago)
        {
            using var connection = new SqlConnection(connectionString);
            var query = @"INSERT INTO Pago 
                          (Id_pago, Id_contrato, Fecha_pago, Monto, Metodo_pago, Archivo_pdf)
                          VALUES (@Id_pago, @Id_contrato, @Fecha_pago, @Monto, @Metodo_pago, @Archivo_pdf)";
            await connection.ExecuteAsync(query, pago);
        }

        public async Task<Pago> ObtenerPorId(string id)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Pago>(
                "SELECT * FROM Pago WHERE Id_pago = @Id", new { Id = id });
        }

        public async Task<IEnumerable<Pago>> ObtenerTodos()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Pago>("SELECT * FROM Pago");
        }

        public async Task Actualizar(Pago pago)
        {
            using var connection = new SqlConnection(connectionString);
            var query = @"UPDATE Pago
                          SET Id_contrato = @Id_contrato, Fecha_pago = @Fecha_pago, 
                              Monto = @Monto, Metodo_pago = @Metodo_pago, Archivo_pdf = @Archivo_pdf
                          WHERE Id_pago = @Id_pago";
            await connection.ExecuteAsync(query, pago);
        }

        public async Task Borrar(string id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE FROM Pago WHERE Id_pago = @Id", new { Id = id });
        }

        public async Task<bool> Existe(string id)
        {
            using var connection = new SqlConnection(connectionString);
            var resultado = await connection.QueryFirstOrDefaultAsync<int>(
                "SELECT 1 FROM Pago WHERE Id_pago = @Id", new { Id = id });
            return resultado == 1;
        }
        public async Task<List<Pago>> ObtenerPagosARealizarPorUsuarioAsync(string idUsuario)
        {
            var pagos = new List<Pago>();

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"  
                       SELECT p.id_pago, p.id_contrato, p.monto_pago, p.descripcion, p.fecha_pago_programada, p.fecha_pago_real, p.Archivo_factura
                       FROM pago p  
                       INNER JOIN contrato c ON p.id_contrato = c.id_contrato  
                       WHERE c.Id_propietario = @IdUsuario";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                pagos.Add(new Pago
                {
                    Id_pago = reader.GetString(0),
                    Id_contrato = reader.GetString(1),
                    Monto_pago = reader.GetDecimal(2),
                    Descripcion = reader.GetString(3),
                    Fecha_pago_programada = reader.GetDateTime(4)
                });
            }
            return pagos;
        }
        public async Task<List<Pago>> ObtenerPagosARecibirPorUsuarioAsync(string idUsuario)
        {
            var pagos = new List<Pago>();

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"  
                       SELECT p.id_pago, p.id_contrato, p.monto_pago, p.descripcion, p.fecha_pago_programada, p.fecha_pago_real, p.Archivo_factura
                       FROM pago p  
                       INNER JOIN contrato c ON p.id_contrato = c.id_contrato  
                       WHERE c.Id_propietario = @IdUsuario";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                pagos.Add(new Pago
                {
                    Id_pago = reader.GetString(0),
                    Id_contrato = reader.GetString(1),
                    Monto_pago = reader.GetDecimal(2),
                    Descripcion = reader.GetString(3),
                    Fecha_pago_programada = reader.GetDateTime(4)
                });
            }
            return pagos;
        }


    }
}