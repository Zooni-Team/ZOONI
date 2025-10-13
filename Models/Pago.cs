public class Pago
{
    public int Id_Pago { get; set; }

    public int Id_Reserva { get; set; }

    public int Id_MetodoPago { get; set; }

    public decimal Monto { get; set; }

    public DateTime Fecha_Pago { get; set; }

    public int Id_EstadoPago { get; set; }
}
    