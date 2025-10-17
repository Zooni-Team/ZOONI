public class Producto
{
    public int Id_Producto { get; set; }
    public string Nombre { get; set; }
    public string Categoria { get; set; } // ejemplo: "Alimentos", "Juguetes", "Accesorios"
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public string ImagenUrl { get; set; }
}
