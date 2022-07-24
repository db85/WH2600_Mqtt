namespace WeatherWH2600MQTT.DataSource
{
    public interface IWetterdataStorage
    {
        void AddData(IEnumerable<KeyValuePair<string, string>> data);
    }
}
