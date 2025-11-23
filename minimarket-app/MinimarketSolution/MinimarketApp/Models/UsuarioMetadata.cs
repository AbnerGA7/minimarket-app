using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MinimarketApp.Models
{
    [ModelMetadataType(typeof(UsuarioMetadata))]
    public partial class Usuario
    {
    }

    public class UsuarioMetadata
    {
        [Display(Name = "Nombre Completo")]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string NombreCompleto { get; set; }

        [Display(Name = "Usuario (Login)")]
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public string Username { get; set; }

        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string PasswordHash { get; set; }

        [Display(Name = "Rol / Cargo")]
        [Required(ErrorMessage = "Selecciona un rol.")]
        public int? IdRol { get; set; }

        [Display(Name = "¿Activo?")]
        public bool? Activo { get; set; }
    }
}