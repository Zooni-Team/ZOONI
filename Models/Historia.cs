namespace Zooni.Models
{
    public class Historia
    {
        public int Id_Historia { get; set; }
        public int Id_User { get; set; }
        public int? Id_Mascota { get; set; }
        public string ImagenUrl { get; set; } = "";
        public string? Texto { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime Expiracion { get; set; }
        public bool Eliminada { get; set; }
        
        // Propiedades adicionales para la vista
        public string? NombreUsuario { get; set; }
        public string? FotoPerfilUsuario { get; set; }
        public string? NombreMascota { get; set; }
        public bool Vista { get; set; }
    }
}

