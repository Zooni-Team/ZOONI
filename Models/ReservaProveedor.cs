using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zooni.Models
{
    [Table("ReservaProveedor")]
    public class ReservaProveedor
    {
        [Key]
        [Column("Id_Reserva")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Reserva { get; set; }

        [Column("Id_User")]
        public int Id_User { get; set; }

        [Column("Id_Proveedor")]
        public int Id_Proveedor { get; set; }

        [Column("Id_Mascota")]
        public int Id_Mascota { get; set; }

        [Column("Id_TipoServicio")]
        public int Id_TipoServicio { get; set; }

        [Column("Fecha_Inicio", TypeName = "datetime2")]
        public DateTime Fecha_Inicio { get; set; }

        [Column("Fecha_Fin", TypeName = "datetime2")]
        public DateTime? Fecha_Fin { get; set; }

        [Column("Hora_Inicio", TypeName = "time")]
        public TimeSpan Hora_Inicio { get; set; }

        [Column("Hora_Fin", TypeName = "time")]
        public TimeSpan? Hora_Fin { get; set; }

        [Column("Duracion_Horas", TypeName = "decimal(5,2)")]
        public decimal? Duracion_Horas { get; set; }

        [Column("Precio_Total", TypeName = "decimal(12,2)")]
        public decimal Precio_Total { get; set; }

        [Column("Id_EstadoReserva")]
        public int Id_EstadoReserva { get; set; } = 1; // 1=Pendiente, 2=Confirmada, 3=EnCurso, 4=Completada, 5=Cancelada

        [StringLength(1000)]
        public string? Notas { get; set; }

        [StringLength(500)]
        [Column("Direccion_Servicio")]
        public string? Direccion_Servicio { get; set; }

        [Column("Latitud_Servicio", TypeName = "decimal(10,8)")]
        public decimal? Latitud_Servicio { get; set; }

        [Column("Longitud_Servicio", TypeName = "decimal(11,8)")]
        public decimal? Longitud_Servicio { get; set; }

        [Column("Compartir_Ubicacion")]
        public bool Compartir_Ubicacion { get; set; } = false;

        [Column("Fecha_Creacion", TypeName = "datetime2")]
        public DateTime Fecha_Creacion { get; set; } = DateTime.Now;

        // Navegación
        public User? User { get; set; }
        public ProveedorServicio? Proveedor { get; set; }
        public Mascota? Mascota { get; set; }
        public TipoServicio? TipoServicio { get; set; }
        public EstadoReserva? EstadoReserva { get; set; }
    }
}

