using Algolia.Search.Clients;
using Algolia.Search.Models.Search;

namespace IncidenciasApp.Services
{
    public class AlgoliaService : IAlgoliaService
    {
        private readonly SearchIndex _index;
        private readonly ILogger<AlgoliaService> _logger;

        public AlgoliaService(IConfiguration configuration, ILogger<AlgoliaService> logger)
        {
            var appId = configuration["Algolia:ApplicationId"];
            var apiKey = configuration["Algolia:ApiKey"]; // Nunca se envia al navegador: se usa solo aqui, en el servidor
            var indexName = configuration["Algolia:IndexName"] ?? "incidencias";

            var client = new SearchClient(appId, apiKey);
            _index = client.InitIndex(indexName);
            _logger = logger;
        }

        public List<AlgoliaIncidenciaDoc> Buscar(string texto)
        {
            try
            {
                var query = new Query(texto) { HitsPerPage = 50 };
                var resultado = _index.Search<AlgoliaIncidenciaDoc>(query);
                return resultado.Hits?.ToList() ?? new List<AlgoliaIncidenciaDoc>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consultando Algolia para el texto '{Texto}'", texto);
                return new List<AlgoliaIncidenciaDoc>();
            }
        }
    }
}
