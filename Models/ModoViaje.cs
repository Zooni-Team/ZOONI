public class ModoViaje
{
    public int Id_ModoViaje { get; set; }
    public int Id_User { get; set; }
    public int Id_Mascota { get; set; }
    public DateTime Fecha_Inicio { get; set; }
    public DateTime Fecha_Fin { get; set; }
    public int Id_Paseador { get; set; }
    public string Notas { get; set; }
}
