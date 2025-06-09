using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManejoAlquileres.Migrations
{
    /// <inheritdoc />
    public partial class UsuWhitInfoBancaria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Informacion_bancaria",
                table: "Usuarios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Informacion_bancaria",
                table: "Usuarios");
        }
    }
}
