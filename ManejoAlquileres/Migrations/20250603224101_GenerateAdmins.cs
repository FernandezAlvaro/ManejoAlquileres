using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ManejoAlquileres.Migrations
{
    /// <inheritdoc />
    public partial class GenerateAdmins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id_usuario", "Apellidos", "Contraseña", "Direccion", "Email", "EsAdministrador", "Informacion_bancaria", "NIF", "Nombre", "Telefono" },
                values: new object[,]
                {
                    { "admin0001", "Administrador Uno", "123456", "Calle Admin 1", "admin1@example.com", true, "111111111111111111", "12345678A", "Admin1", "600000001" },
                    { "admin0002", "Administrador Dos", "123456", "Calle Admin 2", "admin2@example.com", true, "22222222222222222222", "12345678B", "Admin2", "600000002" },
                    { "admin0003", "Administrador Tres", "123456", "Calle Admin 3", "admin3@example.com", true, "33333333333333333333", "12345678C", "Admin3", "600000003" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id_usuario",
                keyValue: "admin0001");

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id_usuario",
                keyValue: "admin0002");

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id_usuario",
                keyValue: "admin0003");
        }
    }
}
