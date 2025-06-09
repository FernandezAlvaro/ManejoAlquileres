using ManejoAlquileres.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authorization;
using ManejoAlquileres.Service.Interface;

namespace ManejoAlquileres.Controllers
{

    public class CuentaController : Controller
    {
        private readonly IServicioCuenta _servicioCuenta;

        public CuentaController(IServicioCuenta sevicioCuenta)
        {
            _servicioCuenta = sevicioCuenta;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View("Registrarse");

        [HttpPost]
        public async Task<IActionResult> Register(Usuario model)
        {
            if (!ModelState.IsValid) return View("Registrarse", model);

            bool registrado = false;
            Usuario nuevoUsuario = null;

            do
            {
                var nuevoId = await _servicioCuenta.GenerarNuevoIdUsuarioAsync();

                nuevoUsuario = new Usuario
                {
                    Id_usuario = nuevoId,
                    Nombre = model.Nombre,
                    Apellidos = model.Apellidos,
                    NIF = model.NIF,
                    Direccion = model.Direccion,
                    Telefono = model.Telefono,
                    Email = model.Email,
                    Contraseña = model.Contraseña,
                    Informacion_bancaria = model.Informacion_bancaria,
                    EsAdministrador = false
                };

                registrado = await _servicioCuenta.RegisterAsync(model);

            } while (!registrado);

            return RedirectToAction("Login");
        }



        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View("Login");
        }

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Login(string email, string contrasena, bool recordar)
        {
            var usuario = await _servicioCuenta.LoginAsync(email, contrasena);

            if (usuario == null)
            {
                ModelState.AddModelError("", "Correo o contraseña incorrectos.");
                return View("Login");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id_usuario),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim("esAdministrador", usuario.EsAdministrador.ToString().ToLower())
            };

            if (usuario.EsAdministrador)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Administrador"));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("Cookies", principal);

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword() => View("ContraseñaOlvidada");

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string dni)
        {
            var (encontrado, email, contrasena) = await _servicioCuenta.ForgotPasswordAsync(dni);
            if (!encontrado)
            {
                ModelState.AddModelError("", "DNI no encontrado.");
                return View("ContraseñaOlvidada");
            }

            ViewBag.Mensaje = $"Tu contraseña es: {contrasena} Por favor, cambia tu contraseña después de iniciar sesión.";

            return View("ContraseñaOlvidada");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}