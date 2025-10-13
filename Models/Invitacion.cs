public class Invitacion
{
    public int Id_Invitacion { get; set; }

    public int Id_Mascota { get; set; }

    public int Id_Emisor { get; set; }

    public int Id_Receptor { get; set; }

    public string Rol { get; set; }  // Ej: Paseador, Tía Marina, Veterinario

    public string Estado { get; set; }  // Pendiente, Aceptada, Rechazada

    public DateTime Fecha { get; set; }
}
