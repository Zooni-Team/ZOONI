public class Comida
{
    public int Id_Comida { get; set; }        // PK de la comida
    public string Nombre { get; set; }        // Nombre de la comida
    public double Calorias { get; set; }      // Calorías por porción
    public double Proteina { get; set; }      // Proteína por porción
    public double Carbohidratos { get; set; } // Carbohidratos por porción
    public double Grasas { get; set; }        // Grasas por porción
    public string Tipo { get; set; }          // Ej: Seco, Húmedo, Snack
}
