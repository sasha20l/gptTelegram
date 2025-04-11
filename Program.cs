using Telegram.Bot;

// Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

// Подключаем контроллеры
builder.Services.AddControllers();

// Настройки бота
const string telegramBotToken = "7899253021:AAEj4L2EIjIpZ4e2o941gjhoUSve17tynto";
builder.Services.AddSingleton(new TelegramBotClient(telegramBotToken));

var app = builder.Build();

// Устанавливаем Webhook (только при запуске — можно обернуть в if или флаг)
await new TelegramBotClient(telegramBotToken)
    .SetWebhookAsync("https://gpttelegram-production.up.railway.app/webhook");

app.UseRouting();
app.MapControllers();

app.Run();

