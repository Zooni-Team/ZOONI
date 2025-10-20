public class RegisterDto
{
    // Mail
    public string Email { get; set; }
    public string Password { get; set; }

    // User
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Pais { get; set; }
    public string Ciudad { get; set; }
    public string Telefono { get; set; }
    public int EdadUsuario { get; set; }

    // Perfil
    public string FotoPerfil { get; set; }
    public string DescripcionPerfil { get; set; }

    // Mascota
    public string PetNombre { get; set; }
    public string PetEspecie { get; set; }
    public string PetRaza { get; set; }
    public string PetColor { get; set; }
    public int PetEdad { get; set; }
    public bool PetEsterilizado { get; set; }
    public string PetChip { get; set; }
    public decimal PetPeso { get; set; }
}
