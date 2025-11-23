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
        // =============================================
        // EXPORTAR INGRESOS DE MERCADERÍA A EXCEL
        // =============================================
        public async Task<IActionResult> ExportarExcelIngresos(DateTime? fecha)
        {
            // Si no eligen fecha, usamos HOY
            DateTime fechaFiltro = fecha ?? DateTime.Today;

            // Buscamos lotes cuya Fecha de Recepción coincida con el día seleccionado (ignorando hora)
            var lotes = await _context.Lotes
                .Include(l => l.IdProductoNavigation)
                .Include(l => l.IdProveedorNavigation)
                .Where(l => l.FechaRecepcion.HasValue && l.FechaRecepcion.Value.Date == fechaFiltro.Date)
                .ToListAsync();

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Ingresos del Día");

                // Cabecera Estilizada
                var header = worksheet.Range("A1:G1");
                header.Style.Font.Bold = true;
                header.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.ForestGreen;
                header.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;

                worksheet.Cell(1, 1).Value = "N° Lote";
                worksheet.Cell(1, 2).Value = "Producto";
                worksheet.Cell(1, 3).Value = "Proveedor";
                worksheet.Cell(1, 4).Value = "Vencimiento";
                worksheet.Cell(1, 5).Value = "Cantidad";
                worksheet.Cell(1, 6).Value = "Costo Unit.";
                worksheet.Cell(1, 7).Value = "Inversión Total";

                int row = 2;
                foreach (var lote in lotes)
                {
                    worksheet.Cell(row, 1).Value = lote.NumeroLote;
                    worksheet.Cell(row, 2).Value = lote.IdProductoNavigation?.Nombre;
                    worksheet.Cell(row, 3).Value = lote.IdProveedorNavigation?.RazonSocial;
                    worksheet.Cell(row, 4).Value = lote.FechaVencimiento.ToDateTime(TimeOnly.MinValue);
                    worksheet.Cell(row, 5).Value = lote.CantidadInicial;

                    worksheet.Cell(row, 6).Value = lote.CostoCompraUnitario;
                    worksheet.Cell(row, 6).Style.NumberFormat.Format = "\"S/\" #,##0.00";

                    // Cálculo del total invertido en ese lote
                    decimal total = lote.CantidadInicial * lote.CostoCompraUnitario;
                    worksheet.Cell(row, 7).Value = total;
                    worksheet.Cell(row, 7).Style.NumberFormat.Format = "\"S/\" #,##0.00";

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Ingresos_{fechaFiltro:yyyyMMdd}.xlsx");
                }
            }
        }
    }
}