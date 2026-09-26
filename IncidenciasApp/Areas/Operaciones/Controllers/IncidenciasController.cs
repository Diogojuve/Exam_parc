using System.Text.Json;
using IncidenciasApp.Data;
using IncidenciasApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace IncidenciasApp.Areas.Operaciones.Controllers
{
    [Area("Operaciones")]
    [Authorize]
    public class IncidenciasController : Controller
    {
        private const string CacheKeyListado = "incidencias:abiertas";

        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly ILogger<IncidenciasController> _logger;

        public IncidenciasController(ApplicationDbContext context, IDistributedCache cache, ILogger<IncidenciasController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            List<Incidencia>? incidencias = null;

            var cacheado = await _cache.GetStringAsync(CacheKeyListado);
            if (!string.IsNullOrEmpty(cacheado))
            {
                incidencias = JsonSerializer.Deserialize<List<Incidencia>>(cacheado);
                _logger.LogInformation("Listado de incidencias leido desde REDIS (cache)");
            }

            if (incidencias == null)
            {
                incidencias = await _context.Incidencias
                    .Where(i => i.Estado == EstadoIncidencia.Abierta)
                    .OrderByDescending(i => i.Id)
                    .ToListAsync();

                _logger.LogInformation("Listado de incidencias leido desde la BASE DE DATOS");

                var opciones = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                };
                await _cache.SetStringAsync(CacheKeyListado, JsonSerializer.Serialize(incidencias), opciones);
            }

            return View(incidencias);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cerrar(int id)
        {
            var incidencia = await _context.Incidencias.FindAsync(id);
            if (incidencia != null && incidencia.Estado == EstadoIncidencia.Abierta)
            {
                incidencia.Estado = EstadoIncidencia.Cerrada;
                incidencia.FechaCierre = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await _cache.RemoveAsync(CacheKeyListado);
                _logger.LogInformation("Incidencia {Id} cerrada; cache del listado invalidada", id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}