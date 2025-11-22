using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MinimarketApp.Models
{
    // 1. Aquí conectamos la clase generada (Proveedore) con su Metadata
    [ModelMetadataType(typeof(ProveedoreMetadata))]
    public partial class Proveedore
    {
    }

    // 2. Aquí definimos la Metadata (Le puse la 'e' también para que coincida)
    public class ProveedoreMetadata
    {
        [Display(Name = "RUC / DNI")]
        [Required(ErrorMessage = "El documento es obligatorio.")]
        public string RucDni { get; set; }

        [Display(Name = "Razón Social")]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string RazonSocial { get; set; }

        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Display(Name = "Dirección")]
        public string Direccion { get; set; }
    }
}