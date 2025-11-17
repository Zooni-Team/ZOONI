namespace Zooni.Models
{
    public class Mencion
    {
        public int Id_Mencion { get; set; }
        public int Id_User_Mencionado { get; set; }
        public int? Id_Publicacion { get; set; }
        public int? Id_Historia { get; set; }
        public int Id_User_Menciona { get; set; }
        public DateTime Fecha { get; set; }
        public bool Vista { get; set; }
        public bool Reposteada { get; set; }
        
        // Propiedades adicionales para la vista
        public string? NombreUsuarioMenciona { get; set; }
        public string? FotoPerfilUsuarioMenciona { get; set; }
        public string? ImagenPublicacion { get; set; }
        public string? ImagenHistoria { get; set; }
        public string? DescripcionPublicacion { get; set; }
    }
}

