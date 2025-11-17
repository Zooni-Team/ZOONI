namespace Zooni.Models
{
    public class ComentarioPublicacion
    {
        public int Id_Comentario { get; set; }
        public int Id_Publicacion { get; set; }
        public int Id_User { get; set; }
        public string Contenido { get; set; } = "";
        public DateTime Fecha { get; set; }
        public bool Eliminado { get; set; }
        
        // Propiedades adicionales para la vista
        public string? NombreUsuario { get; set; }
        public string? FotoPerfilUsuario { get; set; }
    }
}

