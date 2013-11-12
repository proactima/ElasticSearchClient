using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;

namespace ElasticSearchClient
{
    class Program
    {
        static void Main(string[] args)
        {

            var unit = CreateMiniEntity();
            var setting = new ConnectionSettings(new Uri("http://137.117.194.221:9200"));
            setting.SetDefaultIndex("entitystore");

            var client = new ElasticClient(setting);

            // Console.WriteLine("Adding Entities...");
            // 
            //var descriptor = new BulkDescriptor();
            //foreach (var i in Enumerable.Range(100000, 100010))
            //{
            //    unit = CreateMiniEntity(id: i.ToString(), name: i.ToString());

            //    descriptor.Index<dynamic>(op => op.Index("entitystore").Id(i.ToString()).Type("units").Object(unit));
            //}
            //var result = client.Bulk(descriptor);

            string q = "some_1";

            var result = client.Search(body =>
                body.QueryString(q));

            foreach (var r in result.Documents.ToList())
            {
                Console.WriteLine(r);
            }
            Console.WriteLine("Took: " + result.ElapsedMilliseconds);
            Console.WriteLine("Hits: " + result.Hits.Total);


            Console.ReadLine();
        }

        internal static Dictionary<string, object> CreateMiniEntity(string id = "1", string parentId = "1",
            string name = "One", string etag = "some_1", IEnumerable<Dictionary<string, object>> children = null)
        {
            var dict = new Dictionary<string, object>
            {
                {"id", id},
                {"parentId", parentId},
                {"name", name},
                {"url", "http...."},
                {"ETag", etag}
            };

            if (children != null)
                dict.Add("children", children);

            return dict;
        }
    }
}
