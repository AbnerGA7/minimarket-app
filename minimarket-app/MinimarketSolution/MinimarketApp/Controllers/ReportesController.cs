using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimarketApp.Models;
using System.Security.Claims;
using ClosedXML.Excel;
using System.Globalization; // Necesario para CultureInfo
using System.Data; // Necesario para ClosedXML


namespace MinimarketApp.Controllers
{
    // Solo el Rol Administrador puede acceder al controlador de Reportes
    [Authorize(Roles = "Administrador")]
    public class ReportesController : Controller
    {
        private readonly MinimarketContext _context;

        public ReportesController(MinimarketContext context)
        {
            _context = context;
        }

        // Helper para reutilizar la consulta de Ventas
        private async Task<List<Venta>> ObtenerVentas(DateTime inicio, DateTime fin)
        {
            return await _context.Ventas
                .Include(v => v.IdMedioPagoNavigation)
                .Include(v => v.IdUsuarioNavigation)
                .Include(v => v.DetalleVenta)
                    .ThenInclude(d => d.IdLoteNavigation)
                        .ThenInclude(l => l.IdProductoNavigation) // <-- CORREGIDO: Navegación completa
                .Where(v => v.FechaVenta >= inicio && v.FechaVenta <= fin)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
        }

        // =============================================
        // 1. PANTALLA DE REPORTES (GET)
        // =============================================
        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFin)
        {
            DateTime inicio = fechaInicio ?? DateTime.Today;

            // Si el usuario elige una fecha fin, tomamos ese día hasta el último segundo (23:59:59)
            DateTime fechaBaseFin = fechaFin ?? DateTime.Today;
            DateTime fin = fechaBaseFin.Date.AddDays(1).AddTicks(-1);

            ViewBag.FechaInicio = inicio.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaBaseFin.ToString("yyyy-MM-dd");

            var ventas = await ObtenerVentas(inicio, fin);

            // Calcular Totales para las tarjetas
            decimal totalVendido = 0;
            decimal totalCosto = 0;

            foreach (var venta in ventas)
            {
                totalVendido += venta.TotalVenta;
                foreach (var detalle in venta.DetalleVenta)
                {
                    // Evitar nulos
                    decimal costoUnitario = detalle.IdLoteNavigation?.CostoCompraUnitario ?? 0;
                    totalCosto += detalle.Cantidad * costoUnitario;
                }
            }

            ViewBag.TotalVendido = totalVendido;
            ViewBag.TotalCosto = totalCosto;
            ViewBag.GananciaNeta = totalVendido - totalCosto;

            return View(ventas);
        }

        // =============================================
        // 2. EXPORTAR A EXCEL (LÓGICA CLOSEDXML)
        // =============================================
        public async Task<IActionResult> ExportarExcel(DateTime? fechaInicio, DateTime? fechaFin)
        {
            // Lógica de fechas
            DateTime inicio = fechaInicio ?? DateTime.Today;
            DateTime fechaBaseFin = fechaFin ?? DateTime.Today;
            DateTime fin = fechaBaseFin.Date.AddDays(1).AddTicks(-1);

            var ventas = await ObtenerVentas(inicio, fin);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Ganancias y Ventas");

                // ... (Resto del código de exportación de Excel, queda igual pero ya con la referencia a ClosedXML) ...

                // 1. ESTILOS DE CABECERA
                var headerRange = worksheet.Range("A1:H1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thick;

                // 2. CREAR CABECERAS
                worksheet.Cell(1, 1).Value = "N° Ticket";
                worksheet.Cell(1, 2).Value = "Fecha y Hora";
                worksheet.Cell(1, 3).Value = "Medio Pago";
                worksheet.Cell(1, 4).Value = "Producto";
                worksheet.Cell(1, 5).Value = "Cant.";
                worksheet.Cell(1, 6).Value = "Precio Venta";
                worksheet.Cell(1, 7).Value = "Costo Compra";
                worksheet.Cell(1, 8).Value = "Ganancia";

                // 3. LLENAR DATOS
                int row = 2;
                decimal sumaVenta = 0;
                decimal sumaCosto = 0;
                decimal sumaGanancia = 0;

                foreach (var venta in ventas)
                {
                    foreach (var detalle in venta.DetalleVenta)
                    {
                        // Cálculos por fila
                        decimal costoUnit = detalle.IdLoteNavigation?.CostoCompraUnitario ?? 0;
                        decimal ventaTotal = detalle.Cantidad * detalle.PrecioVentaUnitario;
                        decimal costoTotal = detalle.Cantidad * costoUnit;
                        decimal utilidad = ventaTotal - costoTotal;

                        // Acumuladores generales
                        sumaVenta += ventaTotal;
                        sumaCosto += costoTotal;
                        sumaGanancia += utilidad;

                        // Celdas
                        worksheet.Cell(row, 1).Value = venta.IdVenta;
                        worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        worksheet.Cell(row, 2).Value = venta.FechaVenta;

                        worksheet.Cell(row, 3).Value = venta.IdMedioPagoNavigation.Nombre;

                        // Producto (Navegación corregida para obtener el nombre)
                        string prodNombre = detalle.IdLoteNavigation?.IdProductoNavigation?.Nombre ?? "Desconocido";
                        worksheet.Cell(row, 4).Value = prodNombre;

                        worksheet.Cell(row, 5).Value = detalle.Cantidad;
                        worksheet.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Formatos de Moneda
                        worksheet.Cell(row, 6).Value = ventaTotal;
                        worksheet.Cell(row, 6).Style.NumberFormat.Format = "\"S/\" #,##0.00";

                        worksheet.Cell(row, 7).Value = costoTotal;
                        worksheet.Cell(row, 7).Style.NumberFormat.Format = "\"S/\" #,##0.00";
                        worksheet.Cell(row, 7).Style.Font.FontColor = XLColor.Red;

                        worksheet.Cell(row, 8).Value = utilidad;
                        worksheet.Cell(row, 8).Style.NumberFormat.Format = "\"S/\" #,##0.00";

                        if (utilidad >= 0)
                            worksheet.Cell(row, 8).Style.Font.FontColor = XLColor.Green;
                        else
                            worksheet.Cell(row, 8).Style.Font.FontColor = XLColor.Red;

                        row++;
                    }
                }

                // --- FILA DE TOTALES ---
                var rangoTotales = worksheet.Range($"A{row}:H{row}");
                rangoTotales.Style.Font.Bold = true;
                rangoTotales.Style.Fill.BackgroundColor = XLColor.LightGray;
                rangoTotales.Style.Border.TopBorder = XLBorderStyleValues.Double;

                worksheet.Cell(row, 5).Value = "TOTALES GENERALES:";
                worksheet.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                worksheet.Cell(row, 6).Value = sumaVenta;
                worksheet.Cell(row, 6).Style.NumberFormat.Format = "\"S/\" #,##0.00";

                worksheet.Cell(row, 7).Value = sumaCosto;
                worksheet.Cell(row, 7).Style.NumberFormat.Format = "\"S/\" #,##0.00";

                worksheet.Cell(row, 8).Value = sumaGanancia;
                worksheet.Cell(row, 8).Style.NumberFormat.Format = "\"S/\" #,##0.00";

                worksheet.Columns().AdjustToContents(); // Autoajustar ancho

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    string nombreArchivo = $"Ganancias_{inicio:dd-MM}_{fin:dd-MM}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
                }
            }
        }

        // ... (resto del código igual) ...
    }
}