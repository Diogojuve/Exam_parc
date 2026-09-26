using IncidenciasApp.Data;
using IncidenciasApp.Models;
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
        private readonly ILogger<IncidenciasController> _logger;

        public IncidenciasController(ApplicationDbContext context, ILogger<IncidenciasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET /Operaciones/Incidencias
        public async Task<IActionResult> Index()
        {
            var incidencias = await _context.Incidencias
                .Where(i => i.Estado == EstadoIncidencia.Abierta)
                .OrderByDescending(i => i.Id)
                .ToListAsync();

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
