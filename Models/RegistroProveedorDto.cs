using System.ComponentModel.DataAnnotations;

namespace Zooni.Models
{
    public class RegistroProveedorDto
    {
        [Required(ErrorMessage = "El correo es requerido")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Contrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmar contraseña es requerido")]
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarContrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es requerido")]
        [StringLength(20, ErrorMessage = "El DNI no puede exceder 20 caracteres")]
        public string DNI { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "La experiencia es requerida")]
        [Range(0, 100, ErrorMessage = "La experiencia debe estar entre 0 y 100 años")]
        public int Experiencia_Anios { get; set; }

        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "Debe seleccionar al menos un tipo de servicio")]
        public List<int> TiposServicio { get; set; } = new List<int>();

        [Required(ErrorMessage = "Debe seleccionar al menos una especie")]
        public List<string> Especies { get; set; } = new List<string>();

        [StringLength(30)]
        public string? Telefono { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        [StringLength(100)]
        public string? Ciudad { get; set; }

        [StringLength(100)]
        public string? Provincia { get; set; }

        [StringLength(100)]
        public string? Pais { get; set; }

        [Range(0, 999999.99, ErrorMessage = "El precio debe ser un valor válido")]
        public decimal? Precio_Hora { get; set; }
    }
}

