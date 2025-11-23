using Microsoft.AspNetCore.Authorization; // Para la seguridad
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MinimarketApp.Models;

namespace MinimarketApp.Controllers
{
    // 🔒 SEGURIDAD: Solo el Administrador puede entrar aquí
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly MinimarketContext _context;

        public UsuariosController(MinimarketContext context)
        {
            _context = context;
        }

        // 1. LISTA DE USUARIOS
        public async Task<IActionResult> Index()
        {
            var usuarios = _context.Usuarios.Include(u => u.IdRolNavigation);
            return View(await usuarios.ToListAsync());
        }

        // 2. CREAR USUARIO (PANTALLA)
        public IActionResult Create()
        {
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "NombreRol");
            return View();
        }

        // 3. CREAR USUARIO (GUARDAR)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdUsuario,NombreCompleto,Username,PasswordHash,IdRol,Activo")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                // OJO: Aquí deberíamos encriptar la contraseña en un sistema real.
                // Por ahora la guardamos tal cual para aprender.
                usuario.Activo = true; // Siempre activo al crear

                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "NombreRol", usuario.IdRol);
            return View(usuario);
        }

        // 4. EDITAR USUARIO (PANTALLA)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "NombreRol", usuario.IdRol);
            return View(usuario);
        }

        // 5. EDITAR USUARIO (GUARDAR)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUsuario,NombreCompleto,Username,PasswordHash,IdRol,Activo")] Usuario usuario)
        {
            if (id != usuario.IdUsuario) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.IdUsuario)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "NombreRol", usuario.IdRol);
            return View(usuario);
        }

        // 6. ELIMINAR (BAJA LÓGICA: Solo desactivar para no romper historial)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                // En lugar de borrar, lo desactivamos
                usuario.Activo = false;
                _context.Update(usuario);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }
    }
}