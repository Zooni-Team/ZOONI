using System;

namespace Zooni.Models
{
    public class MascotaCompartida
    {
        public int Id_Compartida { get; set; }
        public int Id_Mascota { get; set; }
        public int Id_Propietario { get; set; }
        public int Id_UsuarioCompartido { get; set; }
        public bool Permiso_Edicion { get; set; } = true;
        public DateTime Fecha_Compartida { get; set; }
        public bool Activo { get; set; } = true;
        
        // Navegación
        public Mascota Mascota { get; set; }
        public User Propietario { get; set; }
        public User UsuarioCompartido { get; set; }
    }

    public class SolicitudMascotaCompartida
    {
        public int Id_Solicitud { get; set; }
        public int Id_Mascota { get; set; }
        public int Id_Propietario { get; set; }
        public int Id_Solicitante { get; set; }
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Aceptada, Rechazada
        public DateTime Fecha_Solicitud { get; set; }
        public DateTime? Fecha_Respuesta { get; set; }
        public string Mensaje { get; set; }
        
        // Navegación
        public Mascota Mascota { get; set; }
        public User Propietario { get; set; }
        public User Solicitante { get; set; }
    }
}

