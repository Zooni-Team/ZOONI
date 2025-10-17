public class Comportamiento
{
    public int Id_Comportamiento { get; set; }
    public int Id_Mascota { get; set; }
    public DateTime Fecha { get; set; }
    public string Estado_Animo { get; set; } // ejemplo: "Feliz", "Triste", "Ansioso"
    public string Actividad_Reciente { get; set; }
    public string Notas { get; set; }
}
