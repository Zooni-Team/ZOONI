public class Reserva
{
    public int Id_Reserva { get; set; }

    public int Id_User { get; set; }

    public int Id_Servicio { get; set; }

    public int Id_Mascota { get; set; }

    public DateTime Fecha_Reserva { get; set; }

    public TimeSpan Hora { get; set; }

    public int Id_EstadoReserva { get; set; }

    public decimal Precio_Final { get; set; }
}
