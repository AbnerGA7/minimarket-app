using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MinimarketApp.Models;
using System.Security.Claims; // Necesario para Claims
using System.Text.Json; // Necesario para la serialización
using Microsoft.AspNetCore.Authorization;
using System.Globalization; // Necesario para CultureInfo y NumberStyles
using System; // Para Math y DateTime
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MinimarketApp.Controllers
{
    // Candado de seguridad: solo usuarios logueados
    [Authorize]
    public class VentasController : Controller
    {
        private readonly MinimarketContext _context;

        public VentasController(MinimarketContext context)
        {
            _context = context;
        }

        // Helper: Obtener el ID del usuario logueado de forma segura (Fix para CS8602)
        private int GetUserId()
        {
            // Usa el operador ?. para verificar si User.FindFirst es nulo antes de acceder a .Value
            var userIdClaim = User.FindFirst("IdUsuario");
            return int.TryParse(userIdClaim?.Value, out int id) ? id : 0;
        }

        // =============================================
        // 1. HISTORIAL DE VENTAS (Cajero/Admin)
        // =============================================
        public async Task<IActionResult> Index(string buscarTicket)
        {
            var query = _context.Ventas
                .Include(v => v.IdUsuarioNavigation)
                .Include(v => v.IdMedioPagoNavigation)
                .AsQueryable();

            // Filtro por número de ticket (ID)
            if (!string.IsNullOrEmpty(buscarTicket))
            {
                if (int.TryParse(buscarTicket, out int idVenta))
                {
                    query = query.Where(v => v.IdVenta == idVenta);
                }
            }

            // Si es CAJERO, solo ve sus ventas (seguridad)
            if (User.IsInRole("Cajero"))
            {
                int idCajero = GetUserId();
                query = query.Where(v => v.IdUsuario == idCajero);
            }

            var ventas = await query.OrderByDescending(v => v.FechaVenta).ToListAsync();
            return View(ventas);
        }

        // =============================================
        // 2. PANTALLA DE CAJA (POS) - Valida Apertura
        // =============================================
        public async Task<IActionResult> NuevaVenta()
        {
            // 🔒 RESTRICCIÓN DE FLUJO: Si no hay Apertura de Caja, el Cajero es redirigido
            if (User.IsInRole("Cajero"))
            {
                var ultimoMovimiento = await _context.CajaMovimientos
                    .Where(c => c.IdUsuario == GetUserId())
                    .OrderByDescending(c => c.FechaMovimiento)
                    .FirstOrDefaultAsync();

                // Si el último movimiento no fue Apertura o fue Cierre hoy
                bool cajaAbierta = ultimoMovimiento != null
                                && ultimoMovimiento.TipoMovimiento == "APERTURA"
                                && ultimoMovimiento.FechaMovimiento.Date == DateTime.Today.Date;

                if (!cajaAbierta)
                {
                    // Si no ha abierto caja hoy, lo enviamos a abrir.
                    TempData["MensajeCaja"] = "Debe realizar la Apertura de Caja antes de comenzar a vender.";
                    return RedirectToAction("Apertura", "Caja");
                }
            }

            ViewData["IdMedioPago"] = new SelectList(_context.MediosPagos, "IdMedioPago", "Nombre");

            // 1. Cargamos todos los productos con Lotes para calcular stock
            var productosBase = await _context.Productos
                .Include(p => p.Lotes.Where(l => l.Estado)) // Solo lotes activos
                .ToListAsync();

            // 2. Creamos una lista simple para el JSON de la búsqueda
            var productosParaBusqueda = productosBase
                .Select(p => new {
                    p.IdProducto,
                    p.Nombre,
                    p.PrecioVenta,
                    p.UnidadMedida,
                    p.CodigoBarras,
                    // Calculamos Stock: Suma todos los lotes activos
                    StockTotal = p.Lotes.Sum(l => l.StockActual)
                })
                .ToList();

            // 3. Serializamos el JSON (para inyectar en la vista)
            ViewBag.ProductosJson = JsonSerializer.Serialize(productosParaBusqueda);

            var carrito = ObtenerCarritoSession();
            return View(carrito);
        }

        // =============================================
        // 3. AGREGAR AL CARRITO (GRANEL / UNIDAD)
        // =============================================
        [HttpPost]
        public async Task<IActionResult> AgregarProducto(int idProducto, string cantidadStr)
        {
            // 1. Validar y convertir la cantidad (Acepta decimales como "0.5" o "1,5")
            if (!decimal.TryParse(cantidadStr.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal cantidad))
            {
                TempData["ErrorVenta"] = "Cantidad inválida. Use punto o coma para decimales.";
                return RedirectToAction(nameof(NuevaVenta));
            }

            if (cantidad <= 0)
            {
                TempData["ErrorVenta"] = "La cantidad debe ser mayor a cero.";
                return RedirectToAction(nameof(NuevaVenta));
            }

            var producto = await _context.Productos.FindAsync(idProducto);
            if (producto == null) return NotFound();

            var carrito = ObtenerCarritoSession();

            decimal precioFinal = producto.PrecioVenta;

            var itemExistente = carrito.FirstOrDefault(c => c.IdProducto == idProducto);

            if (itemExistente != null)
            {
                itemExistente.Cantidad += cantidad;
                itemExistente.Cantidad = Math.Round(itemExistente.Cantidad, 3); // Redondeo para Granel
            }
            else
            {
                carrito.Add(new CarritoItem
                {
                    IdProducto = producto.IdProducto,
                    NombreProducto = producto.Nombre,
                    PrecioUnitario = precioFinal,
                    Cantidad = cantidad,
                    UnidadMedida = producto.UnidadMedida // Se guarda si es KG o UNIDAD
                });
            }

            GuardarCarritoSession(carrito);
            return RedirectToAction(nameof(NuevaVenta));
        }

        // 4. QUITAR DEL CARRITO
        public IActionResult QuitarProducto(int idProducto)
        {
            var carrito = ObtenerCarritoSession();
            var item = carrito.FirstOrDefault(c => c.IdProducto == idProducto);
            if (item != null)
            {
                carrito.Remove(item);
                GuardarCarritoSession(carrito);
            }
            return RedirectToAction(nameof(NuevaVenta));
        }

        // 5. DISMINUIR CANTIDAD (Para Granel y Unidad)
        public IActionResult DisminuirCantidad(int idProducto)
        {
            var carrito = ObtenerCarritoSession();
            var item = carrito.FirstOrDefault(c => c.IdProducto == idProducto);

            if (item != null)
            {
                if (item.UnidadMedida == "UNIDAD")
                {
                    item.Cantidad = Math.Max(0, item.Cantidad - 1);
                }
                else // KG (Granel)
                {
                    // Disminuimos 100 gramos (0.1M)
                    item.Cantidad = Math.Max(0, item.Cantidad - 0.1M);
                    item.Cantidad = Math.Round(item.Cantidad, 3); // Redondeo para evitar errores flotantes
                }

                // Eliminar si queda casi nada (umbral 0.001 para granel)
                if (item.Cantidad <= 0.001M) carrito.Remove(item);

                GuardarCarritoSession(carrito);
            }
            return RedirectToAction(nameof(NuevaVenta));
        }

        // 6. ACTUALIZAR PRECIO UNITARIO (Descuento/Ajuste)
        [HttpPost]
        public IActionResult ActualizarPrecioItem(int idProducto, decimal nuevoPrecio)
        {
            var carrito = ObtenerCarritoSession();
            var item = carrito.FirstOrDefault(c => c.IdProducto == idProducto);
            if (item != null)
            {
                item.PrecioUnitario = nuevoPrecio;
            }
            GuardarCarritoSession(carrito);
            return RedirectToAction(nameof(NuevaVenta));
        }

        // =============================================
        // 7. CONFIRMAR VENTA (FIFO: First In, First Out)
        // =============================================
        [HttpPost]
        public async Task<IActionResult> ConfirmarVenta(int IdMedioPago, decimal MontoPagado)
        {
            var carrito = ObtenerCarritoSession();
            if (carrito.Count == 0) return RedirectToAction(nameof(NuevaVenta));

            decimal totalVenta = carrito.Sum(i => i.SubTotal);
            int idCajero = GetUserId();

            // Validar monto pagado
            if (MontoPagado < totalVenta)
            {
                TempData["ErrorVenta"] = "El monto pagado es insuficiente.";
                return RedirectToAction(nameof(NuevaVenta));
            }

            // A. Cabecera de Venta
            Venta nuevaVenta = new Venta
            {
                FechaVenta = DateTime.Now,
                IdMedioPago = IdMedioPago,
                TotalVenta = totalVenta,
                MontoPagado = MontoPagado,
                Vuelto = MontoPagado - totalVenta,
                IdUsuario = idCajero // ASIGNAMOS EL CAJERO LOGUEADO
            };

            _context.Ventas.Add(nuevaVenta);
            await _context.SaveChangesAsync(); // Guardamos para tener el ID

            // B. Detalle y Resta de Stock (Lógica FIFO)
            foreach (var item in carrito)
            {
                decimal cantidadNecesaria = item.Cantidad;

                // Buscamos lotes activos ordenados por FechaVencimiento ASC (FIFO: El más viejo/próximo a vencer primero)
                var lotes = await _context.Lotes
                    .Where(l => l.IdProducto == item.IdProducto && l.StockActual > 0 && l.Estado == true)
                    .OrderBy(l => l.FechaVencimiento)
                    .ToListAsync();

                foreach (var lote in lotes)
                {
                    if (cantidadNecesaria <= 0) break;

                    int cantidadADescontarLote = 0; // Cantidad física a descontar del StockActual (siempre INT)

                    if (item.UnidadMedida == "UNIDAD")
                    {
                        // Lógica UNIDAD
                        int cantidadLoteInt = (int)lote.StockActual;
                        int cantidadNecesariaInt = (int)Math.Floor(cantidadNecesaria);

                        if (cantidadLoteInt >= cantidadNecesariaInt)
                        {
                            // El lote actual cubre toda la necesidad restante
                            cantidadADescontarLote = cantidadNecesariaInt;
                            lote.StockActual -= cantidadADescontarLote;
                            cantidadNecesaria = 0;
                        }
                        else
                        {
                            // El lote actual se agota
                            cantidadADescontarLote = cantidadLoteInt;
                            cantidadNecesaria -= cantidadLoteInt;
                            lote.StockActual = 0;
                        }

                        // Guardamos el detalle vinculado al LOTE específico
                        DetalleVentum detalle = new DetalleVentum
                        {
                            IdVenta = nuevaVenta.IdVenta,
                            IdLote = lote.IdLote,
                            Cantidad = cantidadADescontarLote,
                            PrecioVentaUnitario = item.PrecioUnitario
                        };
                        _context.DetalleVenta.Add(detalle);
                    }
                    else if (item.UnidadMedida == "KG")
                    {
                        // ** SOLUCIÓN GRANEL (HACK DB INT) **
                        // Descontamos 1 unidad entera del lote más viejo por cada item granel vendido.
                        // Esto asegura que el stock (en unidades de bulto/paquete) baje.

                        if (lote.StockActual > 0)
                        {
                            // Solo descontamos 1 unidad de stock por la venta (Ej: 1 bulto de arroz vendido en granel)
                            lote.StockActual -= 1;
                            cantidadADescontarLote = 1;
                            cantidadNecesaria = 0; // Ya se cubrió la venta con el primer lote FIFO
                        }

                        // Guardamos el detalle:
                        // Cantidad: 1 (por la unidad de bulto descontada)
                        // PrecioUnitario: Usamos el SubTotal del carrito para reflejar el monto TOTAL de la venta granel.
                        DetalleVentum detalle = new DetalleVentum
                        {
                            IdVenta = nuevaVenta.IdVenta,
                            IdLote = lote.IdLote,
                            Cantidad = cantidadADescontarLote,
                            PrecioVentaUnitario = item.SubTotal
                        };
                        _context.DetalleVenta.Add(detalle);
                    }
                }
            }

            await _context.SaveChangesAsync(); // Guardar todos los cambios de stock y detalles

            // 8. Limpiar Carrito y Redirigir al Ticket
            HttpContext.Session.Remove("Carrito");

            return RedirectToAction(nameof(Details), new { id = nuevaVenta.IdVenta, nueva = true });
        }


        // 8. VER DETALLE (TICKET)
        public async Task<IActionResult> Details(int? id, bool nueva = false)
        {
            if (id == null) return NotFound();

            var venta = await _context.Ventas
                .Include(v => v.IdMedioPagoNavigation)
                .Include(v => v.IdUsuarioNavigation)
                .Include(v => v.DetalleVenta)
                    .ThenInclude(d => d.IdLoteNavigation)
                        .ThenInclude(l => l.IdProductoNavigation)
                .FirstOrDefaultAsync(m => m.IdVenta == id);

            if (venta == null) return NotFound();

            // 🔒 Seguridad: Si no es Admin, solo puede ver sus propias ventas.
            if (!User.IsInRole("Administrador") && venta.IdUsuario != GetUserId())
            {
                return Forbid();
            }

            ViewBag.EsVentaNueva = nueva;

            return View(venta);
        }

        // 9. ANULAR VENTA (DEVOLUCIÓN DE STOCK)
        [HttpPost]
        public async Task<IActionResult> AnularVenta(int idVenta)
        {
            // Solo Administrador o el mismo Cajero pueden anular.
            var venta = await _context.Ventas
                .Include(v => v.DetalleVenta)
                .FirstOrDefaultAsync(v => v.IdVenta == idVenta);

            if (venta == null) return NotFound();

            if (!User.IsInRole("Administrador") && venta.IdUsuario != GetUserId())
            {
                return Forbid(); // Prohibido
            }

            // 1. Devolver Stock a los Lotes originales
            foreach (var detalle in venta.DetalleVenta)
            {
                var lote = await _context.Lotes.FindAsync(detalle.IdLote);
                if (lote != null)
                {
                    lote.StockActual += detalle.Cantidad; // El stock regresa (en unidades)
                }
            }

            // 2. Eliminar Detalle y Cabecera de Venta (Simplificación)
            _context.DetalleVenta.RemoveRange(venta.DetalleVenta);
            _context.Ventas.Remove(venta);

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"Venta {idVenta} anulada correctamente. Stock devuelto.";
            return RedirectToAction(nameof(Index));
        }

        // 10. CANCELAR VENTA (Limpiar Carrito)
        public IActionResult CancelarVenta()
        {
            HttpContext.Session.Remove("Carrito");
            TempData["MensajeInfo"] = "Venta cancelada. Carrito vacío.";
            return RedirectToAction(nameof(NuevaVenta));
        }

        // 11. HELPERS DE CARRITO (Session)
        private List<CarritoItem> ObtenerCarritoSession()
        {
            var data = HttpContext.Session.GetString("Carrito");
            if (string.IsNullOrEmpty(data)) return new List<CarritoItem>();

            try
            {
                // Manejar posible null después de la deserialización (aunque el List no debería ser nulo aquí)
                var list = JsonSerializer.Deserialize<List<CarritoItem>>(data);
                return list ?? new List<CarritoItem>();
            }
            catch
            {
                return new List<CarritoItem>();
            }
        }

        private void GuardarCarritoSession(List<CarritoItem> carrito)
        {
            string data = JsonSerializer.Serialize(carrito);
            HttpContext.Session.SetString("Carrito", data);
        }
    }
}