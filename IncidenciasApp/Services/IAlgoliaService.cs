namespace IncidenciasApp.Services
{
    public class AlgoliaIncidenciaDoc
    {
        public string ObjectID { get; set; } = string.Empty;
        public string Estacion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public interface IAlgoliaService
    {
        /// <summary>
        /// Busca en el índice de Algolia por texto libre (estación o descripción).
        /// Se ejecuta siempre en el servidor: la clave nunca llega al navegador.
        /// </summary>
        List<AlgoliaIncidenciaDoc> Buscar(string texto);
    }
}
