using IncidenciasApp.Data;
using IncidenciasApp.Models;
using IncidenciasApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IncidenciasApp.Areas.Operaciones.Controllers
{
    [Area("Operaciones")]
    [Authorize]
    public class IncidenciasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAlgoliaService _algolia;
        private readonly ILogger<IncidenciasController> _logger;

        public IncidenciasController(ApplicationDbContext context, IAlgoliaService algolia, ILogger<IncidenciasController> logger)
        {
            _context = context;
            _algolia = algolia;
            _logger = logger;
        }

        // GET /Operaciones/Incidencias?q=texto
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

                _logger.LogInformation("Búsqueda Algolia '{Query}' devolvió {Cantidad} resultados abiertos", q, incidencias.Count);
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
                _logger.LogInformation("Incidencia {Id} cerrada", id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}