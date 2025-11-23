using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimarketApp.Models;
using System.Security.Claims;

namespace MinimarketApp.Controllers
{
    // Aplicamos un candado: Si no está logueado, lo manda al Login
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class HomeController : Controller
    {
        private readonly MinimarketContext _context;

        public HomeController(MinimarketContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // ===============================================
            // 1. LÓGICA DE REDIRECCIÓN Y CONTROL DE FLUJO
            // ===============================================
            // Si el usuario es CAJERO, su primer paso es abrir caja.
            if (User.IsInRole("Cajero"))
            {
                // Redirigimos al método de Apertura/Cierre del CajaController
                return RedirectToAction("Apertura", "Caja");
            }

            // Si es Administrador, sigue viendo el Dashboard normal (KPIs).

            // ===============================================
            // 2. CÁLCULO DE KPIs PARA EL DASHBOARD (Solo visible para Admin)
            // ===============================================
            DateTime inicioDia = DateTime.Today;
            DateTime finDia = DateTime.Today.AddDays(1).AddTicks(-1);

            // Rangos para DateOnly
            DateOnly hoy = DateOnly.FromDateTime(DateTime.Today);
            DateOnly alertaVencimiento = DateOnly.FromDateTime(DateTime.Today.AddDays(30));

            // A. Ventas
            var ventasHoy = await _context.Ventas.Where(v => v.FechaVenta >= inicioDia && v.FechaVenta <= finDia).ToListAsync();
            ViewBag.VentasHoy = ventasHoy.Sum(v => v.TotalVenta);
            ViewBag.TicketsHoy = ventasHoy.Count;

            // B. Alertas de Stock
            var productos = await _context.Productos.Include(p => p.Lotes).ToListAsync();
            var productosConStock = productos.Select(p => new
            {
                p.StockMinimo,
                // Sumamos solo lotes activos
                StockReal = p.Lotes.Where(l => l.Estado).Sum(l => l.StockActual)
            }).ToList();

            ViewBag.ProductosAgotados = productosConStock.Count(p => p.StockReal == 0);
            ViewBag.ProductosBajoStock = productosConStock.Count(p => p.StockReal > 0 && p.StockReal <= p.StockMinimo);

            // C. Alertas de Vencimiento (CORRECCIÓN: Filtrando en memoria)
            var todosLosLotes = await _context.Lotes.ToListAsync();
            var lotesPorVencer = todosLosLotes
                .Count(l => l.FechaVencimiento.DayNumber <= alertaVencimiento.DayNumber &&
                            l.FechaVencimiento.DayNumber >= hoy.DayNumber);
            ViewBag.LotesPorVencer = lotesPorVencer;

            // D. Estado de Caja General
            var ultimoMovimiento = await _context.CajaMovimientos
                .Where(c => c.FechaMovimiento >= inicioDia && c.FechaMovimiento <= finDia)
                .OrderByDescending(c => c.FechaMovimiento)
                .FirstOrDefaultAsync();
            ViewBag.EstadoCaja = (ultimoMovimiento?.TipoMovimiento == "CIERRE") ? "CERRADA" : "PENDIENTE DE CIERRE";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}