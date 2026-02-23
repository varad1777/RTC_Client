using ModbusRtuWebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHostedService<ModbusPollingService>();

var app = builder.Build();

app.MapControllers();
app.Run();
