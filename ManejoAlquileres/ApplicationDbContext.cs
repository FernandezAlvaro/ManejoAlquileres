using ManejoAlquileres.Models;
using Microsoft.EntityFrameworkCore;

namespace ManejoAlquileres
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Contrato> Contratos { get; set; }
        public DbSet<Propiedad> Propiedades { get; set; }
        public DbSet<Habitacion> Habitaciones { get; set; }
        public DbSet<GastoInmueble> GastosInmueble { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<ContratoInquilino> ContratosInquilinos { get; set; }
        public DbSet<ContratoPropietario> ContratosPropietarios { get; set; }
        public DbSet<PropiedadUsuario> PropiedadesUsuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Nombres de tablas
            modelBuilder.Entity<Usuario>().ToTable("Usuarios");
            modelBuilder.Entity<Contrato>().ToTable("Contratos");
            modelBuilder.Entity<Propiedad>().ToTable("Propiedades");
            modelBuilder.Entity<Habitacion>().ToTable("Habitaciones");
            modelBuilder.Entity<GastoInmueble>().ToTable("GastosInmueble");
            modelBuilder.Entity<Pago>().ToTable("Pagos");
            modelBuilder.Entity<ContratoInquilino>().ToTable("ContratosInquilinos");
            modelBuilder.Entity<ContratoPropietario>().ToTable("ContratosPropietarios");
            modelBuilder.Entity<PropiedadUsuario>().ToTable("PropiedadesUsuarios");

            // Precision para decimal
            modelBuilder.Entity<Contrato>()
                .Property(c => c.Fianza)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Contrato>()
                .Property(c => c.Comision_inmobiliaria)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Contrato>()
                .Property(c => c.Porcentaje_incremento)
                .HasPrecision(5, 2);

            modelBuilder.Entity<GastoInmueble>()
                .Property(g => g.Monto_gasto)
                .HasPrecision(18, 2);
            modelBuilder.Entity<GastoInmueble>()
                .Property(g => g.Porcentaje_amortizacion)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Habitacion>()
                .Property(h => h.Tamaño)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Pago>()
                .Property(p => p.Monto_pago)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Propiedad>()
                .Property(p => p.Valor_adqui_total)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Propiedad>()
                .Property(p => p.Valor_adquisicion)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Propiedad>()
                .Property(p => p.Valor_catastral_piso)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Propiedad>()
                .Property(p => p.Valor_catastral_terreno)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PropiedadUsuario>()
                .Property(pu => pu.PorcentajePropiedad)
                .HasPrecision(5, 2);


            // Configuración de las claves primarias compuestas para las tablas intermedias
            modelBuilder.Entity<ContratoInquilino>()
                .HasKey(ci => new { ci.ContratoId, ci.UsuarioId });

            modelBuilder.Entity<ContratoPropietario>()
                .HasKey(cp => new { cp.ContratoId, cp.UsuarioId });

            modelBuilder.Entity<PropiedadUsuario>()
                .HasKey(pu => new { pu.PropiedadId, pu.UsuarioId });

            // Relaciones entre Usuario y Contrato con ContratoInquilino
            modelBuilder.Entity<ContratoInquilino>()
                .HasOne(ci => ci.Contrato)
                .WithMany(c => c.Inquilinos)
                .HasForeignKey(ci => ci.ContratoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContratoInquilino>()
                .HasOne(ci => ci.Usuario)
                .WithMany(u => u.ContratosComoInquilino)
                .HasForeignKey(ci => ci.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relaciones entre Usuario y Contrato con ContratoPropietario
            modelBuilder.Entity<ContratoPropietario>()
                .HasOne(cp => cp.Contrato)
                .WithMany(c => c.Propietarios)
                .HasForeignKey(cp => cp.ContratoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContratoPropietario>()
                .HasOne(cp => cp.Usuario)
                .WithMany(u => u.ContratosComoPropietario)
                .HasForeignKey(cp => cp.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relaciones entre Usuario y PropiedadUsuario
            modelBuilder.Entity<PropiedadUsuario>()
                .HasOne(pu => pu.Usuario)
                .WithMany(u => u.Propiedades)
                .HasForeignKey(pu => pu.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PropiedadUsuario>()
                .HasOne(pu => pu.Propiedad)
                .WithMany(p => p.Usuarios)
                .HasForeignKey(pu => pu.PropiedadId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relaciones entre Contrato y Propiedad
            modelBuilder.Entity<Contrato>()
                .HasOne(c => c.Propiedad)
                .WithMany(p => p.Contratos)
                .HasForeignKey(c => c.Id_propiedad)
                .OnDelete(DeleteBehavior.Restrict);

            // Relaciones entre Contrato y Habitacion
            modelBuilder.Entity<Contrato>()
                .HasOne(c => c.Habitacion)
                .WithMany(h => h.Contratos)
                .HasForeignKey(c => c.Id_habitacion)
                .OnDelete(DeleteBehavior.Restrict);

            // Relaciones entre Pago y Contrato
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Contrato)
                .WithMany(c => c.Pagos)
                .HasForeignKey(p => p.Id_contrato)
                .OnDelete(DeleteBehavior.Cascade);

            // Relaciones entre Habitacion y Propiedad
            modelBuilder.Entity<Habitacion>()
                .HasOne(h => h.Propiedad)
                .WithMany(p => p.Habitaciones)
                .HasForeignKey(h => h.Id_propiedad)
                .OnDelete(DeleteBehavior.Cascade);

            // Relaciones entre GastoInmueble y Propiedad
            modelBuilder.Entity<GastoInmueble>()
                .HasOne(g => g.Propiedad)
                .WithMany(p => p.GastoInmueble)
                .HasForeignKey(g => g.Id_propiedad)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    Id_usuario = "admin0001",
                    Nombre = "Admin1",
                    Apellidos = "Administrador Uno",
                    Contraseña = "123456",
                    NIF = "12345678A",
                    Direccion = "Calle Admin 1",
                    Telefono = "600000001",
                    Email = "admin1@example.com",
                    EsAdministrador = true,
                    Informacion_bancaria = "111111111111111111",
                    ContratosComoInquilino = null,
                    ContratosComoPropietario = null,
                    Propiedades = null
                },
                new Usuario
                {
                    Id_usuario = "admin0002",
                    Nombre = "Admin2",
                    Apellidos = "Administrador Dos",
                    Contraseña = "123456",
                    NIF = "12345678B",
                    Direccion = "Calle Admin 2",
                    Telefono = "600000002",
                    Email = "admin2@example.com",
                    EsAdministrador = true,
                    Informacion_bancaria = "22222222222222222222",
                    ContratosComoInquilino = null,
                    ContratosComoPropietario = null,
                    Propiedades = null
                },
                new Usuario
                {
                    Id_usuario = "admin0003",
                    Nombre = "Admin3",
                    Apellidos = "Administrador Tres",
                    Contraseña = "123456",
                    NIF = "12345678C",
                    Direccion = "Calle Admin 3",
                    Telefono = "600000003",
                    Email = "admin3@example.com",
                    EsAdministrador = true,
                    Informacion_bancaria = "33333333333333333333",
                    ContratosComoInquilino = null,
                    ContratosComoPropietario = null,
                    Propiedades = null
                }
            );
        }
    }
}
