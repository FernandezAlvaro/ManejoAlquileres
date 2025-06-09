using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManejoAlquileres.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Propiedades",
                columns: table => new
                {
                    Id_propiedad = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Referencia_catastral = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    numHabitaciones = table.Column<int>(type: "int", nullable: false),
                    Valor_catastral_piso = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Valor_catastral_terreno = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Fecha_adquisicion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Valor_adquisicion = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Valor_adqui_total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado_propiedad = table.Column<bool>(type: "bit", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Propiedades", x => x.Id_propiedad);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id_usuario = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Contraseña = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    NIF = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EsAdministrador = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id_usuario);
                });

            migrationBuilder.CreateTable(
                name: "GastosInmueble",
                columns: table => new
                {
                    Id_gasto = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Id_propiedad = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Tipo_gasto = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Monto_gasto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Fecha_pago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Porcentaje_amortizacion = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    Repercutible = table.Column<bool>(type: "bit", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GastosInmueble", x => x.Id_gasto);
                    table.ForeignKey(
                        name: "FK_GastosInmueble_Propiedades_Id_propiedad",
                        column: x => x.Id_propiedad,
                        principalTable: "Propiedades",
                        principalColumn: "Id_propiedad",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Habitaciones",
                columns: table => new
                {
                    Id_habitacion = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Id_propiedad = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Tamaño = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Disponible = table.Column<bool>(type: "bit", nullable: false),
                    Bano_propio = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Habitaciones", x => x.Id_habitacion);
                    table.ForeignKey(
                        name: "FK_Habitaciones_Propiedades_Id_propiedad",
                        column: x => x.Id_propiedad,
                        principalTable: "Propiedades",
                        principalColumn: "Id_propiedad",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropiedadesUsuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<string>(type: "nvarchar(9)", nullable: false),
                    PropiedadId = table.Column<string>(type: "nvarchar(9)", nullable: false),
                    PorcentajePropiedad = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropiedadesUsuarios", x => new { x.PropiedadId, x.UsuarioId });
                    table.ForeignKey(
                        name: "FK_PropiedadesUsuarios_Propiedades_PropiedadId",
                        column: x => x.PropiedadId,
                        principalTable: "Propiedades",
                        principalColumn: "Id_propiedad",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropiedadesUsuarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contratos",
                columns: table => new
                {
                    Id_contrato = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Id_propiedad = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Id_habitacion = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    Fecha_inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fecha_fin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tipo_Alquiler = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Porcentaje_incremento = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    Clausula_prorroga = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Fecha_max_revision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fianza = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Comision_inmobiliaria = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Aval = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Periodicidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contratos", x => x.Id_contrato);
                    table.ForeignKey(
                        name: "FK_Contratos_Habitaciones_Id_habitacion",
                        column: x => x.Id_habitacion,
                        principalTable: "Habitaciones",
                        principalColumn: "Id_habitacion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contratos_Propiedades_Id_propiedad",
                        column: x => x.Id_propiedad,
                        principalTable: "Propiedades",
                        principalColumn: "Id_propiedad",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContratosInquilinos",
                columns: table => new
                {
                    ContratoId = table.Column<string>(type: "nvarchar(9)", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(9)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContratosInquilinos", x => new { x.ContratoId, x.UsuarioId });
                    table.ForeignKey(
                        name: "FK_ContratosInquilinos_Contratos_ContratoId",
                        column: x => x.ContratoId,
                        principalTable: "Contratos",
                        principalColumn: "Id_contrato",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContratosInquilinos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContratosPropietarios",
                columns: table => new
                {
                    ContratoId = table.Column<string>(type: "nvarchar(9)", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(9)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContratosPropietarios", x => new { x.ContratoId, x.UsuarioId });
                    table.ForeignKey(
                        name: "FK_ContratosPropietarios_Contratos_ContratoId",
                        column: x => x.ContratoId,
                        principalTable: "Contratos",
                        principalColumn: "Id_contrato",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContratosPropietarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id_pago = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Id_contrato = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Fecha_pago_programada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fecha_pago_real = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Monto_pago = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Archivo_factura = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id_pago);
                    table.ForeignKey(
                        name: "FK_Pagos_Contratos_Id_contrato",
                        column: x => x.Id_contrato,
                        principalTable: "Contratos",
                        principalColumn: "Id_contrato",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_Id_habitacion",
                table: "Contratos",
                column: "Id_habitacion");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_Id_propiedad",
                table: "Contratos",
                column: "Id_propiedad");

            migrationBuilder.CreateIndex(
                name: "IX_ContratosInquilinos_UsuarioId",
                table: "ContratosInquilinos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratosPropietarios_UsuarioId",
                table: "ContratosPropietarios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_GastosInmueble_Id_propiedad",
                table: "GastosInmueble",
                column: "Id_propiedad");

            migrationBuilder.CreateIndex(
                name: "IX_Habitaciones_Id_propiedad",
                table: "Habitaciones",
                column: "Id_propiedad");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_Id_contrato",
                table: "Pagos",
                column: "Id_contrato");

            migrationBuilder.CreateIndex(
                name: "IX_PropiedadesUsuarios_UsuarioId",
                table: "PropiedadesUsuarios",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContratosInquilinos");

            migrationBuilder.DropTable(
                name: "ContratosPropietarios");

            migrationBuilder.DropTable(
                name: "GastosInmueble");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "PropiedadesUsuarios");

            migrationBuilder.DropTable(
                name: "Contratos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Habitaciones");

            migrationBuilder.DropTable(
                name: "Propiedades");
        }
    }
}
