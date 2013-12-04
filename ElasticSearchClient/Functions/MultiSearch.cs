using System.Collections.Generic;
using Nest;
using UXRisk.Lib.Common.Models;

namespace ElasticSearchClient.Functions
{
    public static class MultiSearch
    {
        public static IEnumerable<QueryResponse<JsonObject>> Do(IElasticClient client)
        {
            var d = new MultiSearchDescriptor();

            d.Search<JsonObject>(s => s.Index("entities")
                .Type("locations")
                .ConcreteTypeSelector((o, hit) => typeof (JsonObject))
                .FacetQuery("one", q => q.QueryString(a => a.Query("parentId:59791")))
                .FacetQuery("two", q => q.QueryString(a => a.Query("parentId:7309"))));

            d.Search<JsonObject>(s => s.Index("entities")
                .Type("locations")
                .ConcreteTypeSelector((o, hit) => typeof (JsonObject))
                .FacetQuery("facetName", q => q.QueryString(a => a.Query("parentId:7309"))));

            d.Search<JsonObject>(s => s.Index("entities")
                .Type("locations")
                .ConcreteTypeSelector((o, hit) => typeof (JsonObject))
                .FacetQuery("facetName", q => q.QueryString(a => a.Query("parentId:12711"))));

            d.Search<JsonObject>(s => s.Index("entities")
                .Type("locations")
                .ConcreteTypeSelector((o, hit) => typeof (JsonObject))
                .FacetQuery("facetName", q => q.QueryString(a => a.Query("parentId:60068"))));

            var b = client.MultiSearch(d);

            return b.GetResponses<JsonObject>();
        }
    }
}