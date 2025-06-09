using System.Globalization;
using ManejoAlquileres;
using ManejoAlquileres.Service;
using ManejoAlquileres.Service.Interface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Optivem.Framework.Core.Domain;

var builder = WebApplication.CreateBuilder(args);
// Agregar servicios
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
}); ;

builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar IHttpContextAccessor para acceder al contexto
builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Cuenta/Login";
    options.AccessDeniedPath = "/Cuenta/AccessDenied";
});

// Registrar autenticación por cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";      // Ruta para redirigir si no está autenticado
        options.LogoutPath = "/Cuenta/Logout";    // Ruta para cerrar sesión
        options.Cookie.Name = "MiCookieAuth";     // Nombre de la cookie
        options.ExpireTimeSpan = TimeSpan.FromHours(1);  // Tiempo de expiración
        options.SlidingExpiration = true;          // Renueva la cookie si está cerca de expirar
    });

// Registrar autorización (por defecto)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EsAdministrador", policy =>
        policy.RequireRole("Administrador"));

    options.AddPolicy("UsuarioAutenticado", policy =>
        policy.RequireAuthenticatedUser());
});

// Aquí debes registrar tus servicios e interfaces, por ejemplo:
builder.Services.AddScoped<IServicioUsuarios, ServicioUsuarios>();
builder.Services.AddScoped<IServicioCuenta, ServicioCuenta>();
builder.Services.AddScoped<IServicioContrato, ServicioContrato>();
builder.Services.AddScoped<IServicioPropiedades, ServicioPropiedades>();
builder.Services.AddScoped<IServicioHabitacion, ServicioHabitacion>();
builder.Services.AddScoped<IServicioGastoInmueble, ServicioGastoInmueble>();
builder.Services.AddScoped<IGeneradorIdsService, GeneradorIdsService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mi API Inmobiliaria",
        Version = "v1"
    });
});
builder.Services.AddDistributedMemoryCache(); // Necesario para almacenar la sesión en memoria
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mi API Inmobiliaria v1");
    c.RoutePrefix = "swagger";
});
app.UseStaticFiles();

app.UseRouting();
// Para controladores API (como los de Swagger)
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cuenta}/{action=Login}/{id?}");

app.Run();