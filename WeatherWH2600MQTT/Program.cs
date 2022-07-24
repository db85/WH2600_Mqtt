using Serilog;
using System.Net;
using WeatherWH2600MQTT.DataSource;
using WeatherWH2600MQTT.Mqtt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var configuration = new ConfigurationBuilder()
  .AddJsonFile("./config/appsettings.json")
  .AddJsonFile($"./config/appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
  .Build();

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.File("./logs/log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    );

var hostConfig = configuration.GetSection("Host");
builder.WebHost.UseKestrel(opts =>
{
    opts.ListenAnyIP(hostConfig.GetValue<int>("Port"));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<MQTTBackgroundService>();
builder.Services.AddSingleton<IWetterdataStorage, WetterdataMemoryStorage>();
builder.Services.AddSingleton<IMQTTPublisher, MQTTPublisher>();
builder.Services.AddSingleton<IConfiguration>(s => configuration);



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
