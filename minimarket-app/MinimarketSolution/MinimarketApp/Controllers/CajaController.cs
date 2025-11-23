using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimarketApp.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication; // Necesario para SignOutAsync
using System.Globalization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MinimarketApp.Controllers
{
    // [Authorize] Candado: Solo usuarios logueados pueden acceder (Admin o Cajero)
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class CajaController : Controller
    {
        private readonly MinimarketContext _context;

        public CajaController(MinimarketContext context)
        {
            _context = context;
        }

        // Helper: Obtener el ID del usuario logueado
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst("IdUsuario");
            return int.TryParse(userIdClaim?.Value, out int id) ? id : 0;
        }

        // Helper: Obtener el nombre del usuario logueado
        private string GetUserName()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value ?? "Cajero Desconocido";
        }

        // =============================================
        // 1. APERTURA DE CAJA (GET) - Punto de control al iniciar sesión
        // =============================================
        public async Task<IActionResult> Apertura()
        {
            int idCajero = GetUserId();
            if (idCajero == 0) return RedirectToAction("Salir", "Acceso");

            // Buscar el último movimiento de Apertura o Cierre
            var ultimoMovimiento = await _context.CajaMovimientos
                .Where(c => c.IdUsuario == idCajero)
                .OrderByDescending(c => c.FechaMovimiento)
                .FirstOrDefaultAsync();

            ViewBag.Cajero = GetUserName();

            // Lógica de estado para la vista de Apertura.cshtml
            if (ultimoMovimiento?.TipoMovimiento == "APERTURA" && ultimoMovimiento.FechaMovimiento.Date == DateTime.Today.Date)
            {
                // Si ya abrió hoy, lo mandamos directo al POS para que empiece a vender.
                return RedirectToAction("NuevaVenta", "Ventas");
            }
            else if (ultimoMovimiento?.TipoMovimiento == "CIERRE" && ultimoMovimiento.FechaMovimiento.Date == DateTime.Today.Date)
            {
                // Si ya cerró hoy, lo informamos.
                ViewBag.Estado = "CERRADA";
            }
            else
            {
                // Si el último movimiento no fue Apertura o fue en un día anterior
                ViewBag.Estado = "PENDIENTE";
            }

            // Si necesita abrir, mostramos la vista de Apertura.
            return View();
        }

        // =============================================
        // 2. APERTURA DE CAJA (POST)
        // =============================================
        [HttpPost]
        public async Task<IActionResult> GuardarApertura(decimal montoInicial, string observaciones)
        {
            int idCajero = GetUserId();
            CajaMovimiento apertura = new CajaMovimiento
            {
                FechaMovimiento = DateTime.Now,
                TipoMovimiento = "APERTURA",
                Monto = montoInicial,
                Observacion = observaciones,
                IdUsuario = idCajero
            };
            _context.CajaMovimientos.Add(apertura);
            await _context.SaveChangesAsync();

            // Éxito: Lo mandamos directo al POS
            return RedirectToAction("NuevaVenta", "Ventas");
        }

        // =============================================
        // 3. PANTALLA DE ARQUEO / CIERRE (GET) - (Reporte del Cajero)
        // =============================================
        public async Task<IActionResult> Cierre()
        {
            int idCajero = GetUserId();

            // 1. Determinar el inicio del turno (Fecha de la última APERTURA)
            var apertura = await _context.CajaMovimientos
                .Where(c => c.IdUsuario == idCajero && c.TipoMovimiento == "APERTURA")
                .OrderByDescending(c => c.FechaMovimiento)
                .FirstOrDefaultAsync();

            if (apertura == null) return RedirectToAction(nameof(Apertura));

            DateTime inicioTurno = apertura.FechaMovimiento;
            decimal saldoInicial = apertura.Monto;

            // 2. CÁLCULOS
            decimal ventasEfectivo = await _context.Ventas
                .Include(v => v.IdMedioPagoNavigation)
                .Where(v => v.FechaVenta >= inicioTurno && v.IdUsuario == idCajero
                            && v.IdMedioPagoNavigation.Nombre.ToUpper() == "EFECTIVO")
                .SumAsync(v => v.TotalVenta);

            decimal retirosGastos = await _context.CajaMovimientos
                .Where(c => c.IdUsuario == idCajero && c.FechaMovimiento >= inicioTurno && c.TipoMovimiento == "RETIRO")
                .SumAsync(c => c.Monto);

            // CÁLCULO MANUAL DEL SALDO ESPERADO
            decimal saldoEsperadoCalculado = saldoInicial + ventasEfectivo - retirosGastos;

            // 3. Obtener ventas para el detalle en la vista (Tabla del Reporte)
            var ventasDelTurno = await _context.Ventas
                .Include(v => v.IdMedioPagoNavigation)
                .Where(v => v.FechaVenta >= inicioTurno && v.IdUsuario == idCajero)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();

            // 4. Llenar el View Model
            CajaReporteVM model = new CajaReporteVM
            {
                CajeroNombre = GetUserName(),
                IdCajero = idCajero,
                FechaInicioTurno = inicioTurno,
                SaldoInicial = saldoInicial,
                TotalEfectivoVentas = ventasEfectivo,
                TotalVirtualVentas = await _context.Ventas
                    .Include(v => v.IdMedioPagoNavigation)
                    .Where(v => v.FechaVenta >= inicioTurno && v.IdUsuario == idCajero && v.IdMedioPagoNavigation.Nombre.ToUpper() != "EFECTIVO")
                    .SumAsync(v => v.TotalVenta),
                TotalRetirosYGastos = retirosGastos,
                // ASIGNAMOS EL VALOR CALCULADO AQUÍ
                SaldoEsperado = saldoEsperadoCalculado,
                VentasDelTurno = ventasDelTurno,
                YaExisteCierre = await _context.CajaMovimientos.AnyAsync(c => c.IdUsuario == idCajero && c.TipoMovimiento == "CIERRE" && c.FechaMovimiento >= inicioTurno)
            };

            return View(model);
        }

        // =============================================
        // 4. GUARDAR EL CIERRE (POST)
        // =============================================
        [HttpPost]
        public async Task<IActionResult> GuardarCierre(decimal montoFinal, string observaciones)
        {
            int idCajero = GetUserId();
            if (idCajero == 0) return RedirectToAction("Salir", "Acceso");

            var apertura = await _context.CajaMovimientos
                .Where(c => c.IdUsuario == idCajero && c.TipoMovimiento == "APERTURA")
                .OrderByDescending(c => c.FechaMovimiento)
                .FirstOrDefaultAsync();

            if (apertura == null) return RedirectToAction(nameof(Apertura)); // Seguridad

            CajaMovimiento cierre = new CajaMovimiento
            {
                FechaMovimiento = DateTime.Now,
                TipoMovimiento = "CIERRE",
                Monto = montoFinal, // Monto que el cajero contó (Real)
                Observacion = observaciones,
                IdUsuario = idCajero
            };

            _context.CajaMovimientos.Add(cierre);
            await _context.SaveChangesAsync();

            // Después de cerrar, el usuario debe salir para empezar un nuevo turno
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Acceso");
        }

        // =============================================
        // 5. REGISTRAR INGRESO/RETIRO (GASTOS)
        // =============================================
        public IActionResult Movimiento(string tipo) // tipo será "INGRESO" o "RETIRO"
        {
            ViewBag.TipoMovimiento = tipo;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GuardarMovimiento(string tipoMovimiento, decimal monto, string observaciones)
        {
            int idCajero = GetUserId();
            if (idCajero == 0) return RedirectToAction("Salir", "Acceso");

            var apertura = await _context.CajaMovimientos
                .Where(c => c.IdUsuario == idCajero && c.TipoMovimiento == "APERTURA")
                .OrderByDescending(c => c.FechaMovimiento)
                .FirstOrDefaultAsync();

            var cierre = await _context.CajaMovimientos
                .Where(c => c.IdUsuario == idCajero && c.TipoMovimiento == "CIERRE")
                .OrderByDescending(c => c.FechaMovimiento)
                .FirstOrDefaultAsync();

            // Si la caja no está abierta o ya se cerró hoy, prohibido registrar movimientos
            if (apertura == null || (cierre != null && cierre.FechaMovimiento > apertura.FechaMovimiento))
            {
                ViewBag.TipoMovimiento = tipoMovimiento;
                ViewData["MensajeError"] = "La caja debe estar ABIERTA para registrar movimientos.";
                return View(nameof(Movimiento));
            }

            if (monto <= 0)
            {
                ViewBag.TipoMovimiento = tipoMovimiento;
                ViewData["MensajeError"] = "El monto debe ser positivo.";
                return View(nameof(Movimiento));
            }

            CajaMovimiento movimiento = new CajaMovimiento
            {
                FechaMovimiento = DateTime.Now,
                TipoMovimiento = tipoMovimiento.ToUpper(), // INGRESO o RETIRO
                Monto = monto,
                Observacion = observaciones,
                IdUsuario = idCajero
            };

            _context.CajaMovimientos.Add(movimiento);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}