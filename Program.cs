using System.Net.Http.Headers;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using YarnBot;
using YarnBot.Data;
using YarnBot.Services;
using YarnBot.Repo;
using Serilog;

Log.Logger = new LoggerConfiguration()
.MinimumLevel.Information().MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
.WriteTo.Console().WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30, shared: true).CreateLogger();
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog();
builder.Services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["OneC:BaseUrl"]!);

    var login = builder.Configuration["OneC:Login"];
    var password = builder.Configuration["OneC:Password"];

    var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{login}:{password}"));

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
});
builder.Services.AddSingleton<ITelegramBotClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var token = config["Telegram:Token"] ?? throw new InvalidOperationException("Token can't be initialized");

    return new TelegramBotClient(token);
});
builder.Services.AddScoped<PromoService>();
builder.Services.AddSingleton<MessagesService>();
builder.Services.AddScoped<UserRepo>();
builder.Services.AddScoped<Handler>();
builder.Services.AddScoped<AdminNotifierSerive>();
builder.Services.AddHostedService<BotService>();
builder.Services.AddHostedService<FollowupService>();

var host = builder.Build();

using(var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
    await db.Database.MigrateAsync();
}

await host.RunAsync();
