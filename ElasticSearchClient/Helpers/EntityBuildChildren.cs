using System;
using System.Collections.Generic;
using UXRisk.Lib.Common.Models;

namespace ElasticSearchClient.Helpers
{
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