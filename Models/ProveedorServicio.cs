using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zooni.Models
{
    [Table("ProveedorServicio")]
    public class ProveedorServicio
    {
        [Key]
        [Column("Id_Proveedor")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Proveedor { get; set; }

        [Column("Id_User")]
        public int Id_User { get; set; }

        [Required]
        [StringLength(20)]
        public string DNI { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Column("NombreCompleto")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Column("Experiencia_Anios")]
        public int Experiencia_Anios { get; set; }

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        [StringLength(500)]
        [Column("FotoPerfil")]
        public string? FotoPerfil { get; set; }

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

        // Campos de ubicación para zona de atención
        [Column(TypeName = "decimal(10,8)")]
        public decimal? Latitud { get; set; }

        [Column(TypeName = "decimal(11,8)")]
        public decimal? Longitud { get; set; }

        [Column("Radio_Atencion_Km", TypeName = "decimal(10,2)")]
        public decimal? Radio_Atencion_Km { get; set; } = 5.00M; // Radio en kilómetros

        [StringLength(20)]
        [Column("Tipo_Ubicacion")]
        public string? Tipo_Ubicacion { get; set; } = "Cobertura"; // "Cobertura" (paseadores) o "Precisa" (cuidadores)

        [Column("Precio_Hora", TypeName = "decimal(12,2)")]
        public decimal? Precio_Hora { get; set; }

        [Column("Calificacion_Promedio", TypeName = "decimal(4,2)")]
        public decimal Calificacion_Promedio { get; set; } = 0;

        [Column("Cantidad_Resenas")]
        public int Cantidad_Resenas { get; set; } = 0;

        public bool Estado { get; set; } = true;

        [Column("Fecha_Registro")]
        public DateTime Fecha_Registro { get; set; } = DateTime.Now;

        public bool Verificado { get; set; } = false;

        // Navegación
        public User? User { get; set; }
        public List<int>? TiposServicio { get; set; }
        public List<string>? Especies { get; set; }
    }
}

