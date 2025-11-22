using System;
using System.Collections.Generic;

namespace MinimarketApp.Models;

public partial class CajaMovimiento
{
    public int IdMovimiento { get; set; }

    public int? IdUsuario { get; set; }

    public DateTime? FechaMovimiento { get; set; }

    public string TipoMovimiento { get; set; } = null!;

    public decimal Monto { get; set; }

    public string? Observacion { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
