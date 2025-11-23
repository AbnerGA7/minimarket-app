using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MinimarketApp.Models
{
    [ModelMetadataType(typeof(CajaMovimientoMetadata))]
    public partial class CajaMovimiento
    {
    }

    public class CajaMovimientoMetadata
    {
        [Display(Name = "Usuario Responsable")]
        public int? IdUsuario { get; set; }

        [Display(Name = "Fecha/Hora")]
        public DateTime FechaMovimiento { get; set; }

        [Display(Name = "Tipo")]
        [Required(ErrorMessage = "El tipo de movimiento es obligatorio.")]
        public string TipoMovimiento { get; set; }

        [Display(Name = "Monto")]
        [Required(ErrorMessage = "El monto es obligatorio.")]
        public decimal Monto { get; set; }

        [Display(Name = "Observación")]
        public string? Observacion { get; set; }
    }
}