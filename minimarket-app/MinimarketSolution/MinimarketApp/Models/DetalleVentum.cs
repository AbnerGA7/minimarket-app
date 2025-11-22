using System;
using System.Collections.Generic;

namespace MinimarketApp.Models;

public partial class DetalleVentum
{
    public int IdDetalle { get; set; }

    public int? IdVenta { get; set; }

    public int? IdLote { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioVentaUnitario { get; set; }

    public decimal? SubTotal { get; set; }

    public virtual Lote? IdLoteNavigation { get; set; }

    public virtual Venta? IdVentaNavigation { get; set; }
}
