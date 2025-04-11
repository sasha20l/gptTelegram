using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

const string telegramBotToken = "7899253021:AAEj4L2EIjIpZ4e2o941gjhoUSve17tynto";
const string groqApiKey = "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";
const string groqModel = "llama3-70b-8192";
const string groqApiUrl = "https://api.groq.com/openai/v1/chat/completions";

var botClient = new TelegramBotClient(telegramBotToken);

// Устанавливаем Webhook на Railway
string webhookUrl = "https://gpttelegram-production.up.railway.app/webhook";


await botClient.SetWebhookAsync(webhookUrl);

app.MapPost("/webhook", async (Update update) =>
{
  if (update.Type != UpdateType.Message || update.Message?.Text == null)
    return;

  var chatId = update.Message.Chat.Id;
  var messageText = update.Message.Text;

  var http = new HttpClient();
  http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", groqApiKey);

  var body = new
  {
    model = groqModel,
    messages = new[] { new { role = "user", content = messageText } }
  };

  var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
  var response = await http.PostAsync(groqApiUrl, content);
  var json = await response.Content.ReadAsStringAsync();

  string reply = "⚠️ Ошибка.";
  using var doc = JsonDocument.Parse(json);
  if (doc.RootElement.TryGetProperty("choices", out var choices))
  {
    reply = choices[0].GetProperty("message").GetProperty("content").GetString() ?? reply;
  }

  await botClient.SendTextMessageAsync(chatId, reply);
});

app.Run();


