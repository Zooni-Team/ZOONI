namespace Zooni.Models
{
    public class CompartirPublicacion
    {
        public int Id_Compartir { get; set; }
        public int Id_Publicacion { get; set; }
        public int Id_User { get; set; }
        public DateTime Fecha { get; set; }
    }
}

