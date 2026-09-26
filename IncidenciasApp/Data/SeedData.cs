using IncidenciasApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IncidenciasApp.Data
{
    public static class SeedData
    {
        public static async Task InicializarAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

            const string adminEmail = "admin@incidencias.com";
            const string adminPassword = "Admin123!";

            if (await userManager.FindByEmailAsync(adminEmail) is null)
            {
                var admin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, adminPassword);
            }

            if (!await context.Incidencias.AnyAsync())
            {
                context.Incidencias.AddRange(
                    new Incidencia { Estacion = "Estación Parque Central", Descripcion = "Rueda delantera pinchada en bicicleta 12", Prioridad = PrioridadIncidencia.Alta, Estado = EstadoIncidencia.Abierta },
                    new Incidencia { Estacion = "Estación Universidad", Descripcion = "Freno trasero no responde", Prioridad = PrioridadIncidencia.Alta, Estado = EstadoIncidencia.Abierta },
                    new Incidencia { Estacion = "Estación Malecón", Descripcion = "Asiento roto en dos unidades", Prioridad = PrioridadIncidencia.Media, Estado = EstadoIncidencia.Abierta },
                    new Incidencia { Estacion = "Estación Plaza Mayor", Descripcion = "Candado electrónico no cierra", Prioridad = PrioridadIncidencia.Alta, Estado = EstadoIncidencia.Abierta },
                    new Incidencia { Estacion = "Estación Terminal", Descripcion = "Cadena oxidada, requiere lubricación", Prioridad = PrioridadIncidencia.Baja, Estado = EstadoIncidencia.Abierta },
                    new Incidencia { Estacion = "Estación Mercado", Descripcion = "Estación sin conexión a internet", Prioridad = PrioridadIncidencia.Media, Estado = EstadoIncidencia.Cerrada, FechaCierre = DateTime.UtcNow.AddDays(-1) }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
