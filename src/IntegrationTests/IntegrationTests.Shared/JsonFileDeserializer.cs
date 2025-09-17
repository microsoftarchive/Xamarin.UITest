using System.IO;
using System.Runtime.Serialization.Json;

namespace IntegrationTests.Shared
{
    public class JsonFileDeserializer
    {
        public T DeserializeConfigurationFile<T>(string filePath) where T : class, new()
        {
            var fileStream = File.OpenRead(filePath);
            var serializer = new DataContractJsonSerializer(typeof(T));
            return (T)serializer.ReadObject(fileStream);
        }
    }
}