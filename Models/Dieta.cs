public class Dieta
{
    public int Id_Dieta { get; set; }         // PK de la dieta
    public int Id_Mascota { get; set; }       // FK a la mascota
    public string Nombre { get; set; }        // Nombre de la dieta
    public string Descripcion { get; set; }   // Ej: "Dieta baja en grasas"
    public DateTime FechaInicio { get; set; } // Fecha de inicio
    public DateTime FechaFin { get; set; }   // Fecha de fin (opcional)
    
    // Lista de comidas asignadas a esta dieta
    public List<Comida> Comidas { get; set; } = new List<Comida>();
}
