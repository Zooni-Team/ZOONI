using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zooni.Models
{
    [Table("Mail")]
    public class Mail
    {
        [Key]
        [Column("Id_Mail")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Mail { get; set; }

        [Required, StringLength(150)]
        public string Correo { get; set; }

        [Required, StringLength(100)]
        public string Contrasena { get; set; }

        public DateTime Fecha_Creacion { get; set; } = DateTime.Now;
        public DateTime? Ultimo_Acceso { get; set; }
    }
}
