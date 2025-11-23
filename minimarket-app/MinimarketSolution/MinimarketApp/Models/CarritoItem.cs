using System.ComponentModel.DataAnnotations;

namespace MinimarketApp.Models
{
    // Esta clase NO va a la base de datos, es solo para la pantalla de Ventas
    public class CarritoItem
    {
        public int IdProducto { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Cantidad { get; set; } // <--- DECIMAL para soportar KG (0.5, 0.25)

        // CORRECCIÓN: Damos valores iniciales para eliminar los warnings CS8618
        public string NombreProducto { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;

        // Calculamos el subtotal automáticamente (Precio * Cantidad)
        public decimal SubTotal => PrecioUnitario * Cantidad;
    }
}