using System;
using System.Collections.Generic;

namespace MinimarketApp.Models;

public partial class CajaMovimiento
{
    public int IdMovimiento { get; set; }

    public int? IdUsuario { get; set; }

    // CORRECCIÓN: Quitamos el '?' para evitar errores de comparación
    // y asumimos que siempre tiene un valor por el DEFAULT GETDATE() en SQL
    public DateTime FechaMovimiento { get; set; }

    public string TipoMovimiento { get; set; } = null!; // APERTURA, CIERRE, INGRESO, RETIRO

    public decimal Monto { get; set; }

    public string? Observacion { get; set; }

    // Propiedad de navegación (Relación con la tabla Usuarios)
    public virtual Usuario? IdUsuarioNavigation { get; set; }
}