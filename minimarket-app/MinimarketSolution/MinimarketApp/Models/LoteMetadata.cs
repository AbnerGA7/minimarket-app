using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MinimarketApp.Models
{
    [ModelMetadataType(typeof(LoteMetadata))]
    public partial class Lote
    {
    }

    public class LoteMetadata
    {
        // --- PRODUCTO Y PROVEEDOR ---
        [Display(Name = "Producto")]
        [Required(ErrorMessage = "Debes seleccionar un producto.")]
        public int? IdProducto { get; set; }

        [Display(Name = "Proveedor")]
        [Required(ErrorMessage = "Debes seleccionar quién te vendió esto.")]
        public int? IdProveedor { get; set; }

        // --- DATOS DEL LOTE ---
        [Display(Name = "N° de Lote")]
        [Required(ErrorMessage = "Ingresa el código de lote (está en la caja/empaque).")]
        public string NumeroLote { get; set; }

        [Display(Name = "Fecha de Vencimiento")]
        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria.")]
        [DataType(DataType.Date)] // Esto hace que salga un calendario en el navegador
        public DateTime FechaVencimiento { get; set; }

        // --- COSTOS Y CANTIDADES ---
        [Display(Name = "Costo Unitario Compra (S/)")]
        [Required(ErrorMessage = "Ingresa cuánto te costó cada unidad.")]
        [Range(0.01, 10000, ErrorMessage = "El costo debe ser mayor a 0.")]
        public decimal CostoCompraUnitario { get; set; }

        [Display(Name = "Cantidad Recibida")]
        [Required(ErrorMessage = "Ingresa la cantidad que llegó.")]
        [Range(1, 10000, ErrorMessage = "La cantidad debe ser al menos 1.")]
        public int CantidadInicial { get; set; }

        // Stock Actual no se pide en el formulario, se calcula solo, pero le ponemos nombre bonito para el reporte
        [Display(Name = "Stock Actual")]
        public int StockActual { get; set; }
    }
}