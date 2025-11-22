using System;
using System.Collections.Generic;

namespace MinimarketApp.Models;

public partial class Proveedore
{
    public int IdProveedor { get; set; }

    public string? RucDni { get; set; }

    public string RazonSocial { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? Direccion { get; set; }

    public virtual ICollection<Lote> Lotes { get; set; } = new List<Lote>();
}
