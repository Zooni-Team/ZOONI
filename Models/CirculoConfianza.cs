public class CirculoConfianza
{
    public int Id_Circulo { get; set; }
    public int Id_User { get; set; }
    public int Id_Amigo { get; set; }
    public string Rol { get; set; } // ejemplo: "Paseador", "Veterinario", "Dueño", "Amigo"
    public double Latitud { get; set; }
    public double Longitud { get; set; }
    public DateTime UltimaConexion { get; set; }
}
