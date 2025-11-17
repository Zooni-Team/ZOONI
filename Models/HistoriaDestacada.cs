namespace Zooni.Models
{
    public class HistoriaDestacada
    {
        public int Id_Destacada { get; set; }
        public int Id_User { get; set; }
        public int Id_Historia { get; set; }
        public string? Titulo { get; set; }
        public DateTime Fecha { get; set; }
        
        // Propiedades adicionales para la vista
        public string? ImagenUrl { get; set; }
    }
}

