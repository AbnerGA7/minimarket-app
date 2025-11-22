using System;
using System.Collections.Generic;

namespace MinimarketApp.Models;

public partial class Lote
{
    public int IdLote { get; set; }

    public int? IdProducto { get; set; }

    public int? IdProveedor { get; set; }

    public string? NumeroLote { get; set; }

    public DateOnly FechaVencimiento { get; set; }

    public DateTime? FechaRecepcion { get; set; }

    public decimal CostoCompraUnitario { get; set; }

    public int CantidadInicial { get; set; }

    public int StockActual { get; set; }

    // CORRECCIÓN AQUÍ: Se quitó el '?' para que sea obligatorio (True/False)
    public bool Estado { get; set; }

    public virtual ICollection<DetalleVentum> DetalleVenta { get; set; } = new List<DetalleVentum>();

    public virtual Producto? IdProductoNavigation { get; set; }

    public virtual Proveedore? IdProveedorNavigation { get; set; }
}