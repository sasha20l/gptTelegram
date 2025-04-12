using Telegram.Bot;


var builder = WebApplication.CreateBuilder(args);

// Подключаем конфигурацию
builder.Configuration.AddJsonFile("appsettings.Production.json", optional: true);

string telegramToken = builder.Configuration["Telegram:Token"];
const string webhookUrl = "https://gpttelegram-production.up.railway.app/webhook";

// Добавляем TelegramBotClient
builder.Services.AddSingleton(new TelegramBotClient(telegramToken));
builder.Services.AddControllers().AddNewtonsoftJson(); // Для Update

var app = builder.Build();

// Устанавливаем webhook при запуске
var botClient = app.Services.GetRequiredService<TelegramBotClient>();
await botClient.DeleteWebhookAsync(); // на всякий случай
await botClient.SetWebhookAsync(webhookUrl);

app.UseRouting();
app.MapControllers();

app.Run();
