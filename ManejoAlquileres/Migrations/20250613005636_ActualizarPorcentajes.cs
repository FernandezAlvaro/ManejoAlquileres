using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManejoAlquileres.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarPorcentajes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Contratos SET Porcentaje_incremento = Porcentaje_incremento * 100");
            migrationBuilder.Sql("UPDATE GastosInmueble SET Porcentaje_amortizacion = Porcentaje_amortizacion * 100");
            migrationBuilder.Sql("UPDATE PropiedadesUsuarios SET PorcentajePropiedad = PorcentajePropiedad * 100");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Contratos SET Porcentaje_incremento = Porcentaje_incremento / 100");
            migrationBuilder.Sql("UPDATE GastosInmueble SET Porcentaje_amortizacion = Porcentaje_amortizacion / 100");
            migrationBuilder.Sql("UPDATE PropiedadesUsuarios SET PorcentajePropiedad = PorcentajePropiedad / 100");

        }
    }
}
