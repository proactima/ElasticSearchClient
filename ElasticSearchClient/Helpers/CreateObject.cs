using UXRisk.Lib.Common.Models;

namespace ElasticSearchClient.Helpers
{
    public static class CreateObject
    {
        public static JsonObject CreateMiniEntity(string id = "1", string parentId = "1",
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
}