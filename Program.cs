using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ==== Настройки токенов и URL ====
const string telegramBotToken = "7899253021:AAEj4L2EIjIpZ4e2o941gjhoUSve17tynto";
const string groqApiKey = "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";
const string groqModel = "llama3-70b-8192";
const string groqApiUrl = "https://api.groq.com/openai/v1/chat/completions";

// Подставь Railway-домен, который ты получил:
const string webhookUrl = "https://gpttelegram-production.up.railway.app/webhook";

// ==== Инициализация Telegram клиента ====
var botClient = new TelegramBotClient(telegramBotToken);
await botClient.DeleteWebhookAsync(); // на всякий случай очистим
await botClient.SetWebhookAsync(webhookUrl);

// ==== Обработка входящих Webhook ====
app.MapPost("/webhook", async (HttpContext httpContext) =>
{
  try
  {
    var update = await JsonSerializer.DeserializeAsync<Update>(httpContext.Request.Body);
    if (update is not { Type: UpdateType.Message, Message.Text: not null }) return;

    var chatId = update.Message.Chat.Id;
    var messageText = update.Message.Text;

    Console.WriteLine($"📩 Пользователь: {messageText}");

    await botClient.SendTextMessageAsync(chatId, "✍️ Думаю...");

    var aiResponse = await AskGroqAsync(messageText);
    await botClient.SendTextMessageAsync(chatId, aiResponse ?? "❌ Ошибка от Groq");
  }
  catch (Exception ex)
  {
    Console.WriteLine($"❌ Ошибка: {ex.Message}");
  }
});

// ==== Метод запроса к Groq ====
async Task<string?> AskGroqAsync(string prompt)
{
  using var http = new HttpClient();
  http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", groqApiKey);

  var body = new
  {
    model = groqModel,
    messages = new[] { new { role = "user", content = prompt } }
  };

  var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
  var response = await http.PostAsync(groqApiUrl, content);
  var json = await response.Content.ReadAsStringAsync();

  Console.WriteLine("📦 Ответ Groq:\n" + json);

  using var doc = JsonDocument.Parse(json);
  if (doc.RootElement.TryGetProperty("choices", out var choices))
    return choices[0].GetProperty("message").GetProperty("content").GetString();

  if (doc.RootElement.TryGetProperty("error", out var err))
    return "❌ Groq: " + err.GetProperty("message").GetString();

  return "❓ Неизвестный ответ от Groq.";
}

app.Run();

