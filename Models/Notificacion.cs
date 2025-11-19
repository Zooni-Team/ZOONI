public class Notificacion
{
    public int Id_Notificacion { get; set; }

    public int Id_User { get; set; }

    public string Tipo { get; set; }

    public string Titulo { get; set; }

    public string Mensaje { get; set; }

    public int? Id_Referencia { get; set; }

    public string? Url { get; set; }

    public DateTime Fecha { get; set; }

    public Boolean Leida { get; set; }

    public Boolean Eliminada { get; set; }
}
