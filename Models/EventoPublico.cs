namespace Zooni.Models
{
    public class EventoPublico
    {
        public int Id_Evento { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Organizador { get; set; }
        public DateTime Fecha { get; set; }
        public string Hora { get; set; }
        public string Lugar { get; set; }
        public string Imagen { get; set; }
        public string Especie { get; set; }
        public string Raza { get; set; }
        public bool Activo { get; set; }
    }
}
