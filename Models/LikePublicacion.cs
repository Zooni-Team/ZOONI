namespace Zooni.Models
{
    public class LikePublicacion
    {
        public int Id_Like { get; set; }
        public int Id_Publicacion { get; set; }
        public int Id_User { get; set; }
        public DateTime Fecha { get; set; }
    }
}

