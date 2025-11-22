using System;
using System.Collections.Generic;

namespace MinimarketApp.Models;

public partial class MediosPago
{
    public int IdMedioPago { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
