public class Compra
{
    public int Id_Compra { get; set; }
    public int Id_User { get; set; }
    public int Id_Producto { get; set; }
    public int Cantidad { get; set; }
    public DateTime Fecha { get; set; }
    public int Id_Metodo_Pago { get; set; }
}
