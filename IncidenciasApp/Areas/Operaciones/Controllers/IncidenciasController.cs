using System.Text.Json;
using IncidenciasApp.Data;
using IncidenciasApp.Hubs;
using IncidenciasApp.Models;
using IncidenciasApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace IncidenciasApp.Areas.Operaciones.Controllers
{
    [Area("Operaciones")]
    [Authorize]
    public class IncidenciasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAlgoliaService _algolia;
        private readonly IHubContext<IncidenciasHub> _hub;
        private readonly ILogger<IncidenciasController> _logger;

        public IncidenciasController(ApplicationDbContext context, IAlgoliaService algolia, IHubContext<IncidenciasHub> hub, ILogger<IncidenciasController> logger)
        {
            _context = context;
            _algolia = algolia;
            _hub = hub;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? q)
        {
            List<Incidencia> incidencias;

            if (string.IsNullOrWhiteSpace(q))
            {
                incidencias = await _context.Incidencias
                    .Where(i => i.Estado == EstadoIncidencia.Abierta)
                    .OrderByDescending(i => i.Id)
                    .ToListAsync();
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

                await _hub.Clients.All.SendAsync("IncidenciaActualizada", new { id = incidencia.Id, estado = incidencia.Estado.ToString() });
                _logger.LogInformation("Incidencia {Id} cerrada; evento IncidenciaActualizada publicado", id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}