public class Tratamiento
{
    public int Id_Tratamiento { get; set; }
    public int Id_Mascota { get; set; }
    public string Nombre { get; set; }
    public DateTime Fecha_Inicio { get; set; }
    public DateTime? Proximo_Control { get; set; }
    public string Veterinario { get; set; }
}
