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

        // Tablas principales
        public DbSet<User> Usuario { get; set; }
        public DbSet<Mascota> Mascotas { get; set; }
        public DbSet<Perfil> Perfiles { get; set; }
        public DbSet<Mail> Mails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================
            // 🧍 USER
            // ==========================
            modelBuilder.Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Perfil)
                .WithOne(p => p.User)
                .HasForeignKey<Perfil>(p => p.Id_Usuario)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Mail)
                .WithMany()
                .HasForeignKey(u => u.Id_Mail)
                .OnDelete(DeleteBehavior.Restrict);

            // ==========================
            // 🐾 MASCOTA
            // ==========================
            modelBuilder.Entity<Mascota>()
                .HasKey(m => m.Id_Mascota);

            modelBuilder.Entity<Mascota>()
                .HasOne(m => m.User)
                .WithMany() // si querés después: .WithMany(u => u.Mascotas)
                .HasForeignKey(m => m.Id_User)
                .OnDelete(DeleteBehavior.Cascade);

            // ==========================
            // 👤 PERFIL
            // ==========================
            modelBuilder.Entity<Perfil>()
                .HasKey(p => p.Id_Perfil);

            modelBuilder.Entity<Perfil>()
                .HasOne(p => p.User)
                .WithOne(u => u.Perfil)
                .HasForeignKey<Perfil>(p => p.Id_Usuario)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Perfil>()
                .HasMany(p => p.Mascotas)
                .WithOne()
                .OnDelete(DeleteBehavior.NoAction);

            // ==========================
            // ✉️ MAIL
            // ==========================
            modelBuilder.Entity<Mail>()
                .HasKey(m => m.Id_Mail);

            modelBuilder.Entity<Mail>()
                .Property(m => m.Correo)
                .HasMaxLength(255);

            modelBuilder.Entity<Mail>()
                .Property(m => m.Contrasena)
                .HasMaxLength(255);
        }
    }
}
