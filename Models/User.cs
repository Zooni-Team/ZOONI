using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zooni.Models
{
    [Table("User")]
    public class User
    {
        [Key]
        [Column("Id_User")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_User { get; set; }

        // FK con Mail
        public int Id_Mail { get; set; }

        [ForeignKey("Id_Mail")]
        public Mail Mail { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; }

        [Required, StringLength(100)]
        public string Apellido { get; set; }

        public string? DNI { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public string? Provincia { get; set; }
        public string? Pais { get; set; }

        public DateTime? Fecha_Nacimiento { get; set; } = DateTime.Now;
        public DateTime Fecha_Registro { get; set; } = DateTime.Now;

        public int Id_TipoUsuario { get; set; }
        public int Id_Ubicacion { get; set; }
        public bool Estado { get; set; } = true;
    }
}
