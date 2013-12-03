using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nest;
using UXRisk.Lib.Common.Models;

namespace ElasticSearchClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var setting = new ConnectionSettings(new Uri("http://137.135.243.181:9200"))
                .SetDefaultIndex("entities");
            //.MapDefaultTypeNames(t => t
            //    .Add(typeof (JsonObject), "locations"));

            var client = new ElasticClient(setting);

            //client.UpdateSettings()

            var j = client.GetTypeNameFor<JsonObject>();


            Console.WriteLine("Adding Entities...");

            //LoadData(client);

            var sed = new SearchDescriptor<JsonObject>();

            sed.Index("entities").Type("locations").ConcreteTypeSelector((o, hit) => typeof (JsonObject));

            sed.FacetQuery("one", q => q.QueryString(a => a.Query("parentId:59791")));
            sed.FacetQuery("two", q => q.QueryString(a => a.Query("parentId:7309")));

            var res = client.Search(sed);

            //var d = new MultiSearchDescriptor();
            //d.Search<JsonObject>(s => s.Index("entities")
            //    .Type("locations")
            //    .ConcreteTypeSelector((o, hit) => typeof (JsonObject))
            //    .FacetQuery("one", q => q.QueryString(a => a.Query("parentId:59791")))
            //    .FacetQuery("two", q => q.QueryString(a => a.Query("parentId:7309"))));
            
            //d.Search<JsonObject>(s => s.Index("entities")
            //    .Type("locations")
            //    .ConcreteTypeSelector((o, hit) => typeof (JsonObject))
            //    .FacetQuery("facetName", q => q.QueryString(a => a.Query("parentId:7309"))));
            //d.Search<JsonObject>(s => s.Index("entities")
            //    .Type("locations")
            //    .ConcreteTypeSelector((o, hit) => typeof (JsonObject))
            //    .FacetQuery("facetName", q => q.QueryString(a => a.Query("parentId:12711"))));
            //d.Search<JsonObject>(s => s.Index("entities")
            //    .Type("locations")
            //    .ConcreteTypeSelector((o, hit) => typeof (JsonObject))
            //    .FacetQuery("facetName", q => q.QueryString(a => a.Query("parentId:60068"))));

            //var b = client.MultiSearch(d);

            //var c = b.GetResponses<JsonObject>();

            //.Search(s => s.FacetQuery("facetName", q => q.QueryString(a => a.Query("parentId:0")))));


            //Console.WriteLine("Searching for Sta");

            //var first = client.Search<JsonObject>(body => body.QueryString("Sta*").Type("locations"));

            //const string q = "some_1";

            //var result = client.Search(body =>
            //    body.QueryString(q));

            //foreach (var r in result.Documents.ToList())
            //{
            //    Console.WriteLine(r);
            //}
            //Console.WriteLine("Took: " + result.ElapsedMilliseconds);
            //Console.WriteLine("Hits: " + result.Hits.Total);

            Console.ReadLine();
        }

        private static void LoadData(ElasticClient client)
        {
            var r = client.CreateIndex("entities", c => c
                .AddMapping<JsonObject>(m => m
                    .IdField(i => i.SetIndex("not_analyzed"))
                    .TypeName("locations")
                    .Properties(p => p
                        .String(s => s.Name("id"))
                        .String(s => s.Name("name").Index(FieldIndexOption.analyzed).IndexAnalyzer("standard"))
                        .String(s => s.Name("parentId")))
                ));

            var all = new List<JsonObject>();

            var reader = new StreamReader(File.OpenRead(@"c:\temp\countries.csv"), new UTF8Encoding());
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                values[2] = values[2].Replace("\"", "");
                var location = CreateMiniEntity(values[0], values[1], values[2]);

                all.Add(location);
            }

            var allObjects = all.ToDictionary(json => json.Id);

            foreach (var item in all)
            {
                var path = new List<string>();
                if (!string.IsNullOrEmpty(item["parentId"].ToString()))
                {
                    RecGetParent(path, allObjects, item);
                    path.Reverse();
                    path.Add(item["name"].ToString());
                    item.Add("fullPath", string.Join("#.#", path));
                }
                else
                    item.Add("fullPath", string.Empty);
            }

            var insertCount = 0;
            var bulker = new BulkDescriptor();

            for (var index = 0; index < all.Count; index++)
            {
                var item = all[index];
                bulker.Index<JsonObject>(op =>
                    op.Index("entities")
                        .Id(Convert.ToString(item["id"]))
                        .Type("locations")
                        .Object(item));

                insertCount++;

                if (insertCount != 1000 && index != (all.Count - 1)) continue;

                PushToElastic(client, bulker);
                insertCount = 0;
                bulker = new BulkDescriptor();
            }
        }

        private static void RecGetParent(List<string> path, Dictionary<string, JsonObject> allObjects, JsonObject item)
        {
            if (item["parentId"].ToString() == "0") return;

            var parent = allObjects[item["parentId"].ToString()];
            path.Add(parent["name"].ToString());
            RecGetParent(path, allObjects, parent);
        }

        private static void PushToElastic(IElasticClient client, BulkDescriptor bulker)
        {
            var result = client.Bulk(bulker);
            Console.WriteLine("Inserted a bulk: {0}", result.Took);
        }

        internal static JsonObject CreateMiniEntity(string id = "1", string parentId = "1",
            string name = "One")
        {
            var jsonObject = new JsonObject
            {
                {"id", id},
                {"parentId", parentId},
                {"name", name}
            };

            return jsonObject;
        }
    }

    public static class EntityBuildChildren
    {
        internal static void BuildChildren(Dictionary<string, JsonObject> allObjects)
        {
            if (allObjects == null)
                throw new ArgumentNullException("allObjects");

            if (allObjects.Count == 0)
                throw new InvalidOperationException("Dictionary does not contain any elements.");

            const string parentKey = "parentId";
            const string childrenKey = "children";
            const string idKey = "id";

            foreach (var loc in allObjects.Values)
            {
                if (!loc.ContainsKey(parentKey)) continue;

                if (loc[parentKey] == loc[idKey] || String.IsNullOrEmpty(loc[parentKey].ToString())) continue;

                var parent = allObjects[loc[parentKey].ToString()];

                if (!parent.ContainsKey(childrenKey))
                    parent.Add(childrenKey, new List<JsonObject>());

                ((List<JsonObject>) parent[childrenKey]).Add(loc);
            }
        }
    }
}