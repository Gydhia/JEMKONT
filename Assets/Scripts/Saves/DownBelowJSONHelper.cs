using Newtonsoft.Json;

namespace DownBelow.GameData
{
    public static class DownBelowJSONHelper
    {
        /// Provide a configured <see cref="JsonSerializerSettings"/> for any JSON save
        /// </summary>
        /// <returns></returns>
        public static JsonSerializerSettings CreateJSONSettings()
        {
            JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
            jsonSettings.NullValueHandling = NullValueHandling.Ignore;
            jsonSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            jsonSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
            jsonSettings.Formatting = Formatting.Indented;
            return jsonSettings;
        }

        /// <summary>
        /// Provide a configured <see cref="JsonSerializerSettings"/> for <see cref="GameDataContainer"/> JSON save
        /// </summary>
        /// <returns></returns>
        public static JsonSerializerSettings CreateJSONGameDataContainerSettings()
        {
            JsonSerializerSettings jsonSettings = CreateJSONSettings();

            jsonSettings.Converters.Add(new JSONGridConverter());
            jsonSettings.Converters.Add(new JSONStackConverter());

            return jsonSettings;
        }

        /// <summary>
        /// Provide a configured <see cref="JsonSerializerSettings"/> for <see cref="GameDataContainer"/> BSON save
        /// </summary>
        /// <returns></returns>
        public static JsonSerializerSettings CreateBSONGameDataContainerSettings()
        {
            JsonSerializerSettings jsonSettings = CreateJSONGameDataContainerSettings();

            jsonSettings.Formatting = Formatting.None;

            return jsonSettings;
        }
    }
}