using System;
using ElasticSearchClient.Functions;
using Nest;
using UXRisk.Lib.Common.Models;

namespace ElasticSearchClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var setting = new ConnectionSettings(new Uri(""))
                .SetDefaultIndex("entities");

            var client = new ElasticClient(setting);

            DataLoader.LoadData(client);
            var simpleFacet = SimpleFacet.Do(client);
            var simpleSearch = SimpleSearch.Do(client);
            var multiSearch = MultiSearch.Do(client);

            Console.ReadLine();
        }
    }
}