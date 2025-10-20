using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zooni.Models
{
    public class Perfil
    {
        [Key]
        public int Id_Perfil { get; set; }

        [ForeignKey(nameof(User))]
        public int Id_Usuario { get; set; }
        public User User { get; set; }

        [StringLength(255)]
        public string FotoPerfil { get; set; }

        [StringLength(255)]
        public string Descripcion { get; set; }

        public int AniosVigencia { get; set; }

        // Relación con Mascotas
        public ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();
    }
}
