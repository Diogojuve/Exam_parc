using System.Text.Json;
using IncidenciasApp.Data;
using IncidenciasApp.Models;
using IncidenciasApp.Services;
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
        private readonly IAlgoliaService _algolia;
        private readonly IDistributedCache _cache;
        private readonly ILogger<IncidenciasController> _logger;

        public IncidenciasController(ApplicationDbContext context, IAlgoliaService algolia, IDistributedCache cache, ILogger<IncidenciasController> logger)
        {
            _context = context;
            _algolia = algolia;
            _cache = cache;
            _logger = logger;
        }

        // GET /Operaciones/Incidencias?q=texto
        public async Task<IActionResult> Index(string? q)
        {
            List<Incidencia> incidencias;

            if (string.IsNullOrWhiteSpace(q))
            {
                var cacheado = await _cache.GetStringAsync(CacheKeyListado);
                if (!string.IsNullOrEmpty(cacheado))
                {
                    incidencias = JsonSerializer.Deserialize<List<Incidencia>>(cacheado)!;
                    _logger.LogInformation("Listado de incidencias leido desde REDIS (cache)");
                }
                else
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
            }
            else
            {
                var hits = _algolia.Buscar(q);
                var estacionesEncontradas = hits.Select(h => h.Estacion).ToList();

                incidencias = await _context.Incidencias
                    .Where(i => i.Estado == EstadoIncidencia.Abierta && estacionesEncontradas.Contains(i.Estacion))
                    .ToListAsync();

                _logger.LogInformation("Búsqueda Algolia '{Query}' devolvió {Cantidad} resultados abiertos (sin cache)", q, incidencias.Count);
            }

            ViewData["Query"] = q;
            return View(incidencias);
        }

        // POST /Operaciones/Incidencias/Cerrar/5
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