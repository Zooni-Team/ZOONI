using System;
using System.ComponentModel.DataAnnotations;

public class Mail
{
    [Key]
    public int Id_Mail { get; set; }

    [Required, StringLength(255)]
    public string Correo { get; set; }

    [Required, StringLength(255)]
    public string Contrasena { get; set; } 

    public DateTime Fecha_Creacion { get; set; }

    public DateTime Ultimo_Acceso { get; set; }
}