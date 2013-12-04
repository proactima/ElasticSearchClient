using Nest;
using UXRisk.Lib.Common.Models;

namespace ElasticSearchClient.Functions
{
    public static class SimpleSearch
    {
        public static IQueryResponse<JsonObject> Do(IElasticClient client)
        {
            return client.Search<JsonObject>(body => body.QueryString("Sta*").Type("locations"));
        }
    }
}