public class Mensaje
{
    public int Id_Mensaje { get; set; }

    public int Id_Chat { get; set; }

    public int Id_User { get; set; }

    public string Contenido { get; set; }

    public DateTime Fecha { get; set; }

    public bool Leido { get; set; }
}
