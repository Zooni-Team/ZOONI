public class Calendario
{
    public int Id_Calendario { get; set; }   // PK del calendario
    public int Id_User { get; set; }         // FK al usuario dueño del calendario
    public string Nombre { get; set; }       // Nombre del calendario, ej: "Calendario Mascotas"
    public string Descripcion { get; set; }  // Descripción opcional
    public DateTime FechaCreacion { get; set; } // Fecha de creación del calendario
    public bool Activo { get; set; }         // Para saber si está activo o archivado

    // Lista de eventos asociados al calendario
    public List<CalendarioEvento> Eventos { get; set; } = new List<CalendarioEvento>();
}

