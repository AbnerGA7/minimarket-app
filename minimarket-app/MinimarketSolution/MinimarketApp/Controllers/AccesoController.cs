using Microsoft.AspNetCore.Mvc;
using MinimarketApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace MinimarketApp.Controllers
{
    public class AccesoController : Controller
    {
        private readonly MinimarketContext _context;

        public AccesoController(MinimarketContext context)
        {
            _context = context;
        }

        // =============================================
        // 1. PANTALLA DE LOGIN (GET) - Fuerza la vista
        // =============================================
        public IActionResult Login()
        {
            // Siempre mostramos la vista de Login, obligando al usuario a ingresar credenciales
            return View();
        }

        // =============================================
        // 2. PROCESAR LOGIN (POST)
        // =============================================
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // 1. Buscar usuario en la Base de Datos
            // Verificamos que coincida Usuario, Contraseña y que esté Activo
            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == password && u.Activo == true);

            if (usuario == null)
            {
                // Si no existe, mostramos error
                ViewData["Mensaje"] = "Usuario o contraseña incorrectos";
                return View();
            }

            // 2. Crear los "Claims" (Datos de identidad)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                new Claim("IdUsuario", usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Role, usuario.IdRolNavigation.NombreRol) // Rol para autorizaciones
            };

            // 3. Crear la identidad y firmar la cookie de acceso
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            // 4. Todo correcto, bienvenido al sistema
            return RedirectToAction("Index", "Home");
        }

        // =============================================
        // 3. CERRAR SESIÓN (SALIR)
        // =============================================
        public async Task<IActionResult> Salir()
        {
            // Borramos la cookie de autenticación
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Lo mandamos de vuelta a la puerta de entrada
            return RedirectToAction("Login", "Acceso");
        }
    }
}