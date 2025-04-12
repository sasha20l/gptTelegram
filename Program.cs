using Telegram.Bot;


var builder = WebApplication.CreateBuilder(args);

const string telegramToken = "7899253021:AAGpvgkcLgQi3Fp5-5K942jylVi_Xt41oDI";
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
