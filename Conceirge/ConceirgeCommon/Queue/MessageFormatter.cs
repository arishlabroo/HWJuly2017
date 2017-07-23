using Newtonsoft.Json;

namespace ConceirgeCommon.Queue
{
    public interface IMessageFormatter<T>
    {
        string Serialize(T input);

        T DeSerialize(string message);
    }

    public class JsonMessageFormatter<T> : IMessageFormatter<T>
    {
        //private static JsonSerializerSettings settings = new JsonSerializerSettings() { };

        public string Serialize(T input)
        {
            return JsonConvert.SerializeObject(input);
        }

        public T DeSerialize(string message)
        {
            return JsonConvert.DeserializeObject<T>(message);
        }
    }
}