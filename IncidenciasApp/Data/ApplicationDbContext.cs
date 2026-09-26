using IncidenciasApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IncidenciasApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Incidencia> Incidencias => Set<Incidencia>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Incidencia>().Property(i => i.Prioridad).HasConversion<string>();
            builder.Entity<Incidencia>().Property(i => i.Estado).HasConversion<string>();
        }
    }
}
