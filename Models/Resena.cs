using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zooni.Models
{
    [Table("Resena")]
    public class Resena
    {
        [Key]
        [Column("Id_Resena")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Resena { get; set; }

        [Column("Id_User")]
        public int Id_User { get; set; }

        [Column("Id_Servicio")]
        public int? Id_Servicio { get; set; } // Nullable para soportar proveedores

        [Column("Id_Proveedor")]
        public int? Id_Proveedor { get; set; } // Para reseñas de proveedores

        [Column("Id_Reserva")]
        public int? Id_Reserva { get; set; } // Referencia a la reserva del servicio

        [Required]
        [Range(1, 5)]
        public int Calificacion { get; set; }

        [StringLength(1000)]
        public string? Comentario { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        // Navegación
        public User? User { get; set; }
        public Servicio? Servicio { get; set; }
        public ProveedorServicio? Proveedor { get; set; }
        public ReservaProveedor? Reserva { get; set; }
    }
}
