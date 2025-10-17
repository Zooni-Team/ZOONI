public class CalendarioEvento
{
    public int Id_Evento { get; set; }       // PK del evento
    public int Id_User { get; set; }         // FK al usuario dueño del evento
    public int? Id_Mascota { get; set; }     // FK opcional a una mascota
    public string Titulo { get; set; }       // Título del evento
    public string Descripcion { get; set; }  // Descripción del evento
    public DateTime Fecha { get; set; }      // Fecha y hora del evento
    public string Tipo { get; set; }         // Tipo de evento: "Vacuna", "Paseo", etc.
}
