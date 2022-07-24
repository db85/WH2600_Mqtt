using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace WeatherWH2600MQTT.Mqtt
{
    public interface IMQTTPublisher
    {
        Task ConnectAsync();
        Task DisconnectAsync();
        Task PublishAsync(string topic, string payload, bool retainFlag = true, int qos = 1);
    }
    public class MQTTPublisher : IMQTTPublisher
    {
        private readonly ILogger<MQTTPublisher> _logger;
        private readonly IConfiguration _configuration;
        private IManagedMqttClient? Client { get; set; }
        public MQTTPublisher(ILogger<MQTTPublisher> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task ConnectAsync()
        {

            var config = _configuration.GetSection("Mqtt");
            string clientId = Guid.NewGuid().ToString();
            string mqttUrl = config.GetValue<string>("Url");
            string mqttUser = config.GetValue<string>("User");
            string mqttPassword = config.GetValue<string>("Password");
            int mqttPort = config.GetValue<int>("Port");
            bool mqttSecure = config.GetValue<bool>("Secure");

            if (string.IsNullOrEmpty(mqttUrl))
            {
                Console.WriteLine($"Connect Mqtt failed (mqttUrl not set)");
                _logger.LogError($"Mqtt connecting failed: mqttUrl not set");
                return;
            }

            Console.WriteLine($"Connect Mqtt to {mqttUrl}");
            var messageBuilder = new MqttClientOptionsBuilder()
            .WithClientId(clientId)
            .WithCredentials(mqttUser, mqttPassword)
            .WithTcpServer(mqttUrl, mqttPort)
            .WithCleanSession();

            var options = mqttSecure
              ? messageBuilder
                .WithTls()
                .Build()
              : messageBuilder
                .Build();

            var managedOptions = new ManagedMqttClientOptionsBuilder()
              .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
              .WithClientOptions(options)
              .Build();

            Client = new MqttFactory().CreateManagedMqttClient();
            Client.ConnectedAsync += (args) =>
            {
                _logger.LogInformation("Mqtt client connected");
                return Task.CompletedTask;
            };
            Client.ConnectingFailedAsync += (args) =>
            {
                _logger.LogError($"Mqtt connecting failed: {args.Exception.Message}");
                return Task.CompletedTask;
            };
            Client.ApplicationMessageProcessedAsync += (args) =>
            {
                _logger.LogTrace($"Mqtt send message processed: {args.ApplicationMessage.ApplicationMessage.Topic}");
                return Task.CompletedTask;
            };
            Client.ApplicationMessageSkippedAsync += (args) =>
            {
                _logger.LogTrace($"Mqtt message skipped: {args.ApplicationMessage.ApplicationMessage.Topic}");
                return Task.CompletedTask;
            };

            await Client.StartAsync(managedOptions);
        }

        public async Task DisconnectAsync()
        {
            if (Client == null)
                return;
            await Client.StopAsync();
        }

        public async Task PublishAsync(string topic, string payload, bool retainFlag = true, int qos = 1)
        {
            if(Client == null)
            {
                _logger.LogError($"Mqtt publish message failed (Client == null)");
                return;
            }

            _logger.LogTrace($"Mqtt enqueue message: {topic}");
            await Client.EnqueueAsync(new MqttApplicationMessageBuilder()
              .WithTopic(topic)
              .WithPayload(payload)
              .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)qos)
              .WithRetainFlag(retainFlag)
              .Build());
        }
    }
}
