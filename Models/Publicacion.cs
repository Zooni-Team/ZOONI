namespace Zooni.Models
{
    public class Publicacion
    {
        public int Id_Publicacion { get; set; }
        public int Id_User { get; set; }
        public int? Id_Mascota { get; set; }
        public string? ImagenUrl { get; set; }
        public string? Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public bool Anclada { get; set; }
        public DateTime? FechaAnclada { get; set; }
        public bool Eliminada { get; set; }
        
        // Propiedades adicionales para la vista
        public string? NombreUsuario { get; set; }
        public string? FotoPerfilUsuario { get; set; }
        public string? NombreMascota { get; set; }
        public int CantidadLikes { get; set; }
        public int CantidadComentarios { get; set; }
        public int CantidadCompartidos { get; set; }
        public bool MeGusta { get; set; }
        public bool Compartida { get; set; }
    }
}

