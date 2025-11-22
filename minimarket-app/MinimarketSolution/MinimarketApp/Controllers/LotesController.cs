using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MinimarketApp.Models;

namespace MinimarketApp.Controllers
{
    public class LotesController : Controller
    {
        private readonly MinimarketContext _context;

        public LotesController(MinimarketContext context)
        {
            _context = context;
        }

        // GET: Lotes
        public async Task<IActionResult> Index()
        {
            var minimarketContext = _context.Lotes
                .Include(l => l.IdProductoNavigation)
                .Include(l => l.IdProveedorNavigation);
            return View(await minimarketContext.ToListAsync());
        }

        // GET: Lotes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lote = await _context.Lotes
                .Include(l => l.IdProductoNavigation)
                .Include(l => l.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdLote == id);
            if (lote == null)
            {
                return NotFound();
            }

            return View(lote);
        }

        // GET: Lotes/Create
        public IActionResult Create()
        {
            // CORRECCIÓN 1: Mostrar "Nombre" y "RazonSocial" en las listas, no el ID
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "Nombre");
            ViewData["IdProveedor"] = new SelectList(_context.Proveedores, "IdProveedor", "RazonSocial");
            return View();
        }

        // POST: Lotes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdLote,IdProducto,IdProveedor,NumeroLote,FechaVencimiento,CostoCompraUnitario,CantidadInicial")] Lote lote)
        {
            if (ModelState.IsValid)
            {
                // CORRECCIÓN 2: Lógica de negocio automática
                lote.StockActual = lote.CantidadInicial; // El stock inicial es lo que compraste
                lote.FechaRecepcion = DateTime.Now;      // La fecha es hoy
                lote.Estado = true;                      // El lote nace activo

                _context.Add(lote);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Si falla, recargamos las listas (con nombres correctos)
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "Nombre", lote.IdProducto);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedores, "IdProveedor", "RazonSocial", lote.IdProveedor);
            return View(lote);
        }

        // GET: Lotes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lote = await _context.Lotes.FindAsync(id);
            if (lote == null)
            {
                return NotFound();
            }

            // Listas corregidas también en Editar
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "Nombre", lote.IdProducto);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedores, "IdProveedor", "RazonSocial", lote.IdProveedor);
            return View(lote);
        }

        // POST: Lotes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdLote,IdProducto,IdProveedor,NumeroLote,FechaVencimiento,FechaRecepcion,CostoCompraUnitario,CantidadInicial,StockActual,Estado")] Lote lote)
        {
            if (id != lote.IdLote)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lote);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoteExists(lote.IdLote))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdProducto"] = new SelectList(_context.Productos, "IdProducto", "Nombre", lote.IdProducto);
            ViewData["IdProveedor"] = new SelectList(_context.Proveedores, "IdProveedor", "RazonSocial", lote.IdProveedor);
            return View(lote);
        }

        // GET: Lotes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lote = await _context.Lotes
                .Include(l => l.IdProductoNavigation)
                .Include(l => l.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdLote == id);
            if (lote == null)
            {
                return NotFound();
            }

            return View(lote);
        }

        // POST: Lotes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lote = await _context.Lotes.FindAsync(id);
            if (lote != null)
            {
                _context.Lotes.Remove(lote);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LoteExists(int id)
        {
            return _context.Lotes.Any(e => e.IdLote == id);
        }
    }
}