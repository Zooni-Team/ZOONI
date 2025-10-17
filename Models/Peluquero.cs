public class Peluquero
{
    public int Id_Peluquero { get; set; }
    public int Id_User { get; set; }
    public string Nombre { get; set; }
    public string Especialidad { get; set; } // Ej: "Corte", "Baño", "Deslanado"
    public string Telefono { get; set; }
    public string Direccion { get; set; }
    public string Email { get; set; }
    public string Descripcion { get; set; }  // Breve bio o presentación
    public double Calificacion_Promedio { get; set; }
    public string ImagenUrl { get; set; }
}
