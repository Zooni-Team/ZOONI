public class Chat
{
    public int Id_Chat { get; set; }              // PK del chat
    public string Nombre { get; set; }            // Nombre del chat (opcional para grupales)
    public bool EsGrupo { get; set; }             // Si es chat grupal o individual
    public DateTime FechaCreacion { get; set; }   // Fecha de creación del chat

    // Lista de participantes del chat
    public List<ParticipanteChat> Participantes { get; set; } = new List<ParticipanteChat>();

    // Lista de mensajes en el chat
    public List<Mensaje> Mensajes { get; set; } = new List<Mensaje>();
}
