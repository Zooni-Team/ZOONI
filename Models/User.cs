using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zooni.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }  // antes Id_User

        // 🔹 Relación con tabla Mail (opcional)
        [ForeignKey(nameof(Mail))]
        public int? Id_Mail { get; set; }
        public Mail? Mail { get; set; }

        // 🔹 Datos básicos
        [Required, StringLength(100)]
        public string Nombre { get; set; }

        [Required, StringLength(100)]
        public string Apellido { get; set; }

        [StringLength(20)]
        public string DNI { get; set; }

        [StringLength(20)]
        public string Telefono { get; set; }

        [StringLength(255)]
        public string Direccion { get; set; }

        [StringLength(100)]
        public string Ciudad { get; set; }

        [StringLength(100)]
        public string Provincia { get; set; }

        [StringLength(100)]
        public string Pais { get; set; }

        public DateTime Fecha_Nacimiento { get; set; } = DateTime.Now;
        public DateTime Fecha_Registro { get; set; } = DateTime.Now;

        public int Id_TipoUsuario { get; set; }
        public bool Estado { get; set; } = true;

        // 🔹 Credenciales (necesarias para el login)
        [Required, StringLength(150)]
        [EmailAddress]
        public string Email { get; set; }

        [Required, StringLength(100)]
        public string Password { get; set; }

        // 🔹 Relación 1:1 con perfil
        public Perfil? Perfil { get; set; }
    }
}
