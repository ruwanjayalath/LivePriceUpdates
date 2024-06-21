using Amega.LivePriceUpdates.API.BackgroundServices;
using Amega.LivePriceUpdates.API.Extensions;
using Amega.LivePriceUpdates.Contracts.Configuration;
using Amega.LivePriceUpdates.Contracts.Interfaces;
using Amega.LivePriceUpdates.Core.Services;
using Amega.LivePriceUpdates.Providers.CEXProvider;
using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IPriceEventService, PriceEventService>();
builder.Services.AddSingleton<IHttpService, HttpService>();
builder.Services.RegisterAllTypes<ILiveDataWebSocketProvider>([typeof(CEXProviderService).Assembly]);
builder.Services.RegisterAllTypes<ILiveDataRestProvider>([typeof(CEXProviderService).Assembly]);

//Background services
builder.Services.AddHostedService<RestAPIConsumer>();
builder.Services.AddHostedService<WebSocketConsumer>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Caching
builder.Services.AddMemoryCache();

//Configuration
builder.Services.Configure<LivePriceUpdateConfiguration>(
    builder.Configuration.GetSection(LivePriceUpdateConfiguration.SectionName));

builder.Services.Configure<ProviderConfiguration>(
    builder.Configuration.GetSection(ProviderConfiguration.SectionName));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseWebSockets();

await app.RunAsync();
