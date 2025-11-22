using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MinimarketApp.Models
{
    [ModelMetadataType(typeof(ProductoMetadata))]
    public partial class Producto
    {
        // No tocar aquí
    }

    public class ProductoMetadata
    {
        // --- CÓDIGO DE BARRAS ---
        [Display(Name = "Código de Barras")] // <--- ESTO ARREGLA EL NOMBRE EN LA PANTALLA
        [Required(ErrorMessage = "El código es obligatorio.")]
        public string CodigoBarras { get; set; }

        // --- NOMBRE ---
        [Display(Name = "Nombre del Producto")]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; }

        // --- CATEGORÍA (EL PROBLEMA QUE TENÍAS) ---
        [Display(Name = "Categoría")] // <--- ¡AQUÍ ESTÁ LA SOLUCIÓN!
        [Required(ErrorMessage = "Selecciona una categoría.")]
        public int? IdCategoria { get; set; }

        // --- STOCK MÍNIMO ---
        [Display(Name = "Stock Mínimo")]
        [Required(ErrorMessage = "Ingresa el stock mínimo.")]
        public int? StockMinimo { get; set; }

        // --- PRECIO ---
        [Display(Name = "Precio de Venta (S/)")]
        [Required(ErrorMessage = "El precio es obligatorio.")]
        public decimal PrecioVenta { get; set; }

        // --- UNIDAD ---
        [Display(Name = "Unidad de Medida")]
        [Required(ErrorMessage = "Selecciona la unidad.")]
        public string UnidadMedida { get; set; }
    }
}