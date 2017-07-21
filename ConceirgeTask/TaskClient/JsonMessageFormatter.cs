using Newtonsoft.Json;

namespace TaskClient
{

    public interface IMessageFormatter<T>
    {
        string Serialize(T input);

        T Serialize(string message);
    }

    public interface IJsonMessageFormatter<T> : IMessageFormatter<T> { }
    public interface IProtoMessageFormatter<T> : IMessageFormatter<T> { }

    public class JsonMessageFormatter<T> : IJsonMessageFormatter<T>
    {
        //private static JsonSerializerSettings settings = new JsonSerializerSettings() { };

        public string Serialize(T input)
        {
            return JsonConvert.SerializeObject(input);
        }

        public T Serialize(string message)
        {
            return JsonConvert.DeserializeObject<T>(message);
        }
    }
}