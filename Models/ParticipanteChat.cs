public class ParticipanteChat
{
    public int Id_Participante { get; set; }      // PK
    public int Id_Chat { get; set; }              // FK al chat
    public int Id_User { get; set; }              // FK al usuario
    public bool Administrador { get; set; }       // Si es admin del chat grupal
    public DateTime FechaIngreso { get; set; }    // Fecha en que se unió al chat
}
