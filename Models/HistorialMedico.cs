public class HistorialMedico
{
    public int Id_Historial { get; set; }

    public int Id_Mascota { get; set; }

    public int Id_Vet { get; set; }

    public DateTime Fecha { get; set; }

    public string Diagnostico { get; set; }

    public string Tratamiento { get; set; }

    public string Receta { get; set; }

    public string Archivo_Adjunto { get; set; }
}
