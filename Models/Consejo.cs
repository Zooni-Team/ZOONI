public class Consejo
{
    public int Id_Consejo { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    public string Categoria { get; set; } // ejemplo: "Salud", "Comportamiento", "Entrenamiento"
    public DateTime Fecha_Publicacion { get; set; }
}
