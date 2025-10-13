public class Paseo
{
    public int Id_Paseo { get; set; }

    public int Id_Paseador { get; set; }

    public int Id_Mascota { get; set; }

    public DateTime Fecha { get; set; }

    public DateTime Hora_Inicio { get; set; }

    public DateTime Hora_Fin { get; set; }

    public int Duracion { get; set; }

    public string Ruta_GPS { get; set; }

    public string Notas { get; set; }

    public string Estado_Animo { get; set; }
}
