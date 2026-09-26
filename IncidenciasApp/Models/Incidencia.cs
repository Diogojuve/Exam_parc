using System.ComponentModel.DataAnnotations;

namespace IncidenciasApp.Models
{
    public enum EstadoIncidencia
    {
        Abierta,
        Cerrada
    }

    public enum PrioridadIncidencia
    {
        Baja,
        Media,
        Alta
    }

    public class Incidencia
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Estacion { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        public PrioridadIncidencia Prioridad { get; set; } = PrioridadIncidencia.Media;

        public EstadoIncidencia Estado { get; set; } = EstadoIncidencia.Abierta;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaCierre { get; set; }
    }
}
