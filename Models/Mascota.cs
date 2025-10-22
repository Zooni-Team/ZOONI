using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zooni.Models
{
    public class Mascota
    {
        [Key]
        public int Id_Mascota { get; set; }

        [ForeignKey(nameof(User))]
        public int Id_User { get; set; }
        public User User { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(50)]
        public string Especie { get; set; }

        [StringLength(100)]
        public string Raza { get; set; }

        [StringLength(20)]
        public string Sexo { get; set; }

        public int Edad { get; set; }

        public DateTime Fecha_Nacimiento { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Peso { get; set; }

        [StringLength(100)]
        public string Color { get; set; }

        public bool Esterilizado { get; set; }

        [StringLength(50)]
        public string Chip { get; set; }

        [StringLength(255)]
        public string Foto { get; set; }

        public bool Estado { get; set; } = true;

    }
}
