public class User
{
    public int Id_User { get; set; }

    public int Id_Mail { get; set; }

    public string Nombre { get; set; }

    public string Apellido { get; set; }

    public string DNI { get; set; }

    public string Telefono { get; set; }

    public string Direccion { get; set; }

    public string Ciudad { get; set; }

    public string Provincia { get; set; }

    public string Pais { get; set; }

    public DateTime Fecha_Nacimiento { get; set; }

    public DateTime Fecha_Registro { get; set; }

    public int Id_TipoUsuario { get; set; }

    public bool Estado { get; set; }
}
