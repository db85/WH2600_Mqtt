using Microsoft.AspNetCore.Mvc;
using WeatherWH2600MQTT.DataSource;
using WeatherWH2600MQTT.Mqtt;

namespace WeatherWH2600MQTT.Controllers
{
    [ApiController]
    public class WeatherstationController : ControllerBase
    {
        private readonly IWetterdataStorage _wetterdataStorage;
        private readonly IMQTTPublisher _MQTTPublisher;
        private readonly ILogger<WeatherstationController> _logger;
        private readonly IConfiguration _configuration;
        public WeatherstationController(
            IWetterdataStorage wetterdataStorage, 
            IMQTTPublisher mQTTPublisher, 
            ILogger<WeatherstationController> logger,
            IConfiguration configuration)
        {
            _wetterdataStorage = wetterdataStorage;
            _MQTTPublisher = mQTTPublisher;
            _logger = logger;
            _configuration = configuration;
        }


        [HttpGet("/weatherstation/updateweatherstation.php")]
        public string Get()
        {
            _logger.LogTrace($"GET /weatherstation/updateweatherstation.php from {Request.Host}");
            _wetterdataStorage.AddData(HttpContext.Request.Query.Select(p => new KeyValuePair<string, string>(p.Key, string.Join(",", p.Value))));

            var config = _configuration.GetSection("Weatherstation");
            var mqttBaseTopic = config.GetValue<string>("mqttBaseTopic");
            if (!mqttBaseTopic.EndsWith("/"))
                mqttBaseTopic = $"{mqttBaseTopic}/";
            if(string.IsNullOrEmpty(mqttBaseTopic))
            {
                _logger.LogError("mqttBaseTopic base topic is empty");
                return string.Empty;
            }

            foreach (var data in HttpContext.Request.Query)
            {
                string payload = string.Join(",", data.Value);
                _MQTTPublisher.PublishAsync(topic: $"{mqttBaseTopic}{data.Key}", payload: payload, retainFlag: true, qos: 0);
            }
            return string.Empty;
        }
    }
}
