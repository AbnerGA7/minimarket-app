using System;
using System.Collections.Generic;

namespace MinimarketApp.Models;

public partial class Venta
{
    public int IdVenta { get; set; }

    public DateTime? FechaVenta { get; set; }

    public int? IdUsuario { get; set; }

    public int? IdMedioPago { get; set; }

    public decimal TotalVenta { get; set; }

    public decimal? MontoPagado { get; set; }

    public decimal? Vuelto { get; set; }

    public virtual ICollection<DetalleVentum> DetalleVenta { get; set; } = new List<DetalleVentum>();

    public virtual MediosPago? IdMedioPagoNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
