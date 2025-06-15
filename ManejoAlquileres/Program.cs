using System.Globalization;
using System.IO;
using System.Runtime.Loader;
using ManejoAlquileres;
using ManejoAlquileres.Service;
using ManejoAlquileres.Service.Interface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Optivem.Framework.Core.Domain;
using DinkToPdf;
using DinkToPdf.Contracts;
using System.Reflection;
using ManejoAlquileres.Utils;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;

var builder = WebApplication.CreateBuilder(args);

// --- Cargar la DLL nativa manualmente para DinkToPdf ---
var env = builder.Environment;
var context = new CustomAssemblyLoadContext();
var wkHtmlToPdfPath = Path.Combine(env.ContentRootPath, "wwwroot", "native", "libwkhtmltox.dll");
context.LoadUnmanagedLibrary(wkHtmlToPdfPath);

// --- Configuración de servicios ---

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Cuenta/Login";
    options.AccessDeniedPath = "/Cuenta/AccessDenied";
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";
        options.LogoutPath = "/Cuenta/Logout";
        options.Cookie.Name = "MiCookieAuth";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EsAdministrador", policy =>
        policy.RequireRole("Administrador"));

    options.AddPolicy("UsuarioAutenticado", policy =>
        policy.RequireAuthenticatedUser());
});

// Servicios propios
builder.Services.AddScoped<IServicioContrato, ServicioContrato>();
builder.Services.AddScoped<IServicioCuenta, ServicioCuenta>();
builder.Services.AddScoped<IServicioGastoInmueble, ServicioGastoInmueble>();
builder.Services.AddScoped<IServicioHabitacion, ServicioHabitacion>();
builder.Services.AddScoped<IServicioPago, ServicioPago>();
builder.Services.AddScoped<IServicioUsuarios, ServicioUsuarios>();
builder.Services.AddScoped<IServicioPropiedades, ServicioPropiedades>();
builder.Services.AddScoped<IGeneradorIdsService, GeneradorIdsService>();
builder.Services.AddScoped<IContratoInquilino, ServicioContratoInquilino>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IPropiedadUsuarioService, PropiedadUsuarioService>();
builder.Services.AddScoped<IContratoPropietario, ServicioContratoPropiedad>();
builder.Services.AddScoped<IHtmlToPdfConverter, HtmlToPdfConverter>();

// Registro para DinkToPdf (IMPORTANTE: después de cargar la DLL)
builder.Services.AddSingleton<IConverter>(new SynchronizedConverter(new PdfTools()));
builder.Services.AddSingleton<IHtmlToPdfConverter, HtmlToPdfConverter>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mi API Inmobiliaria",
        Version = "v1"
    });
});

builder.Services.AddDistributedMemoryCache();

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

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cuenta}/{action=Login}/{id?}");

app.Run();