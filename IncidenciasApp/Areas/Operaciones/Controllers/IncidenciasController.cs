using System.Text;
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
        private const string PieSocketChannel = "incidencias";

        private readonly ApplicationDbContext _context;
        private readonly IAlgoliaService _algolia;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<IncidenciasController> _logger;

        public IncidenciasController(ApplicationDbContext context, IAlgoliaService algolia, IDistributedCache cache, IConfiguration config, IHttpClientFactory httpClientFactory, ILogger<IncidenciasController> logger)
        {
            _context = context;
            _algolia = algolia;
            _cache = cache;
            _config = config;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

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
            }

            ViewData["Query"] = q;
            ViewData["PieSocketApiKey"] = _config["PieSocket:ApiKey"];
            ViewData["PieSocketCluster"] = _config["PieSocket:Cluster"];
            return View(incidencias);
        }

        [HttpGet]
        public async Task<IActionResult> EstadoActual(int id)
        {
            var incidencia = await _context.Incidencias.FindAsync(id);
            if (incidencia == null) return NotFound();
            return Json(new { id = incidencia.Id, estado = incidencia.Estado.ToString() });
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
                _logger.LogInformation("Incidencia {Id} cerrada; cache invalidada", id);

                await PublicarEnPieSocketAsync(incidencia.Id, incidencia.Estado.ToString());
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PublicarEnPieSocketAsync(int id, string estado)
        {
            try
            {
                var apiKey = _config["PieSocket:ApiKey"];
                var cluster = _config["PieSocket:Cluster"];
                var url = $"https://{cluster}.piesocket.com/v3/{PieSocketChannel}?api_key={apiKey}";

                var payload = JsonSerializer.Serialize(new
                {
                    evento = "IncidenciaActualizada",
                    id,
                    estado
                });

                var client = _httpClientFactory.CreateClient();
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);

                _logger.LogInformation("Evento IncidenciaActualizada publicado en PieSocket. Status: {Status}", response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publicando evento en PieSocket");
            }
        }
    }
}
