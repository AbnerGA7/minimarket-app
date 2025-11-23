using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MinimarketApp.Models // <--- ESTE NAMESPACE DEBE COINCIDIR CON TU PROYECTO
{
    // Esta clase transporta el resumen de caja del turno del cajero.
    public class CajaReporteVM
    {
        // Datos del Cajero y Turno
        public string CajeroNombre { get; set; } = string.Empty;
        public int IdCajero { get; set; }
        public DateTime FechaInicioTurno { get; set; } = DateTime.Now;

        // Totales Financieros
        public decimal SaldoInicial { get; set; } = 0m;
        public decimal TotalEfectivoVentas { get; set; } = 0m;
        public decimal TotalVirtualVentas { get; set; } = 0m;
        public decimal TotalRetirosYGastos { get; set; } = 0m;

        // El Saldo Esperado lo calculamos en el controlador y lo asignamos aquí
        public decimal SaldoEsperado { get; set; } // <--- Propiedad regular, NO CALCULADA

        // Lista de todas las ventas del turno (para la tabla inferior)
        public List<Venta> VentasDelTurno { get; set; } = new List<Venta>();

        // Estado del Cierre (Para inhabilitar el botón si ya cerró)
        public bool YaExisteCierre { get; set; }
    }
}