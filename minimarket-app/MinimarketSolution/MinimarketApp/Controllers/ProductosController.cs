using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // Necesario para Include y ToListAsync
using MinimarketApp.Models;

namespace MinimarketApp.Controllers
{
    public class ProductosController : Controller
    {
        private readonly MinimarketContext _context;

        public ProductosController(MinimarketContext context)
        {
            _context = context;
        }

        // =============================================
        // 1. LISTA DE PRODUCTOS (CON BÚSQUEDA Y FILTROS)
        // =============================================
        public async Task<IActionResult> Index(string buscar, int? idCategoria)
        {
            // 1. Consulta Base (Traemos Categoría y Lotes para calcular stock)
            var query = _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.Lotes) // Incluimos lotes para sumar el stock real
                .AsQueryable();

            // 2. Filtro por Buscador (Nombre o Código)
            if (!string.IsNullOrEmpty(buscar))
            {
                query = query.Where(p => p.Nombre.Contains(buscar) || p.CodigoBarras.Contains(buscar));
            }

            // 3. Filtro por Categoría
            if (idCategoria.HasValue)
            {
                query = query.Where(p => p.IdCategoria == idCategoria);
            }

            // 4. Cargar listas para la vista
            ViewData["IdCategoria"] = new SelectList(_context.Categorias, "IdCategoria", "Nombre", idCategoria);
            ViewData["BusquedaActual"] = buscar;

            var listaProductos = await query.ToListAsync();
            return View(listaProductos);
        }

        // ... (El resto de métodos Create, Edit, Details, Delete quedan IGUAL) ...

        // (Solo asegúrate de no borrar el resto del controlador)

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .FirstOrDefaultAsync(m => m.IdProducto == id);
            if (producto == null) return NotFound();
            return View(producto);
        }

        public IActionResult Create()
        {
            ViewData["IdCategoria"] = new SelectList(_context.Categorias, "IdCategoria", "Nombre"); // Corregido para mostrar nombre
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdProducto,CodigoBarras,Nombre,IdCategoria,StockMinimo,PrecioVenta,UnidadMedida")] Producto producto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCategoria"] = new SelectList(_context.Categorias, "IdCategoria", "Nombre", producto.IdCategoria);
            return View(producto);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();
            ViewData["IdCategoria"] = new SelectList(_context.Categorias, "IdCategoria", "Nombre", producto.IdCategoria);
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProducto,CodigoBarras,Nombre,IdCategoria,StockMinimo,PrecioVenta,UnidadMedida")] Producto producto)
        {
            if (id != producto.IdProducto) return NotFound();
            if (ModelState.IsValid)
            {
                try { _context.Update(producto); await _context.SaveChangesAsync(); }
                catch (DbUpdateConcurrencyException) { if (!ProductoExists(producto.IdProducto)) return NotFound(); else throw; }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCategoria"] = new SelectList(_context.Categorias, "IdCategoria", "Nombre", producto.IdCategoria);
            return View(producto);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .FirstOrDefaultAsync(m => m.IdProducto == id);
            if (producto == null) return NotFound();
            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null) { _context.Productos.Remove(producto); }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.IdProducto == id);
        }
    }
}