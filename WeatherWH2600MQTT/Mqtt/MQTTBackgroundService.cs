namespace WeatherWH2600MQTT.Mqtt
{
    public class MQTTBackgroundService : IHostedService
    {
        private readonly IMQTTPublisher _MQTTPublisher;

        public MQTTBackgroundService(IMQTTPublisher mQTTPublisher)
        {
            _MQTTPublisher = mQTTPublisher;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("MQTTBackgroundService Start");
            await _MQTTPublisher.ConnectAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("MQTTBackgroundService Stop");
            await _MQTTPublisher.DisconnectAsync();
        }
    }
}
