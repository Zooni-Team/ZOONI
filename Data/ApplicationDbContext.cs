using Microsoft.EntityFrameworkCore;
using Zooni.Models;

namespace Zooni.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    
        // Tablas (DbSet)
        public DbSet<User> Usuario { get; set; }
        public DbSet<Perfil> Perfiles { get; set; }
        public DbSet<Mascota> Mascotas { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Pago> Pagos { get; set; }
    }
}