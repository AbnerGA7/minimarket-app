using System;
using System.Collections.Generic;

namespace MinimarketApp.Models;

public partial class Producto
{
    public int IdProducto { get; set; }

    public string? CodigoBarras { get; set; }

    public string Nombre { get; set; } = null!;

    public int? IdCategoria { get; set; }

    public int? StockMinimo { get; set; }

    public decimal PrecioVenta { get; set; }

    public string? UnidadMedida { get; set; }

    public string? ImagenUrl { get; set; }

    public virtual Categoria? IdCategoriaNavigation { get; set; }

    public virtual ICollection<Lote> Lotes { get; set; } = new List<Lote>();
}
