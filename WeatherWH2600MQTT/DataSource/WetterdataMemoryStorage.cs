
namespace WeatherWH2600MQTT.DataSource
{
    public class WetterdataMemoryStorage : IWetterdataStorage
    {
        private Queue<IEnumerable<KeyValuePair<string, string>>> Memory { get; set; } = new Queue<IEnumerable<KeyValuePair<string, string>>>();
        public void AddData(IEnumerable<KeyValuePair<string, string>> data)
        {
            Memory.Enqueue(data);
            if (Memory.Count > 100)
                Memory.Dequeue();
        }
    }
}
