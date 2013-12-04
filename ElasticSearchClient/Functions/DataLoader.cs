using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ElasticSearchClient.Helpers;
using Nest;
using UXRisk.Lib.Common.Models;

namespace ElasticSearchClient.Functions
{
    public static class DataLoader
    {
        public static void LoadData(IElasticClient client)
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
                if (line == null) continue;

                var values = line.Split(',');
                values[2] = values[2].Replace("\"", "");
                var location = CreateObject.CreateMiniEntity(values[0], values[1], values[2]);

                all.Add(location);
            }

            var allObjects = all.ToDictionary(json => json.Id);

            foreach (var item in all)
            {
                var path = new List<string>();
                if (!String.IsNullOrEmpty(item["parentId"].ToString()))
                {
                    RecGetParent(path, allObjects, item);
                    path.Reverse();
                    path.Add(item["name"].ToString());
                    item.Add("fullPath", String.Join("#.#", path));
                }
                else
                    item.Add("fullPath", String.Empty);
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

                //PushToElastic(client, bulker);
                var result = client.Bulk(bulker);
                insertCount = 0;
                bulker = new BulkDescriptor();
            }
        }

        private static void RecGetParent(ICollection<string> path, IReadOnlyDictionary<string, JsonObject> allObjects,
            IReadOnlyDictionary<string, object> item)
        {
            if (item["parentId"].ToString() == "0") return;

            var parent = allObjects[item["parentId"].ToString()];
            path.Add(parent["name"].ToString());
            RecGetParent(path, allObjects, parent);
        }
    }
}