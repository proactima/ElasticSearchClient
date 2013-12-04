using Nest;
using UXRisk.Lib.Common.Models;

namespace ElasticSearchClient.Functions
{
    public static class SimpleFacet
    {
        public static IQueryResponse<JsonObject> Do(IElasticClient client)
        {
            var sed = new SearchDescriptor<JsonObject>();

            sed.Index("entities").Type("locations").ConcreteTypeSelector((o, hit) => typeof (JsonObject));

            sed.FacetQuery("one", q => q.QueryString(a => a.Query("parentId:59791")));
            sed.FacetQuery("two", q => q.QueryString(a => a.Query("parentId:7309")));

            return client.Search(sed);
        }
    }
}