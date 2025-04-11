using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

const string telegramBotToken = "7899253021:AAEj4L2EIjIpZ4e2o941gjhoUSve17tynto";
const string groqApiKey = "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";
const string groqModel = "llama3-70b-8192";
const string groqApiUrl = "https://api.groq.com/openai/v1/chat/completions";

var botClient = new TelegramBotClient(telegramBotToken);

using var cts = new CancellationTokenSource();

var receiverOptions = new ReceiverOptions
{
  AllowedUpdates = new[] { UpdateType.Message }
};

botClient.StartReceiving(
    HandleUpdateAsync,
    HandleErrorAsync,
    receiverOptions,
    cancellationToken: cts.Token
);

Console.WriteLine("🤖 GroqBot (v19 API) запущен. Напиши что-нибудь...");
Console.ReadLine();

async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
{
  if (update.Message is not { Text: { } messageText }) return;

  var chatId = update.Message.Chat.Id;
  Console.WriteLine($"📩 Пользователь: {messageText}");

  await bot.SendTextMessageAsync(chatId, "✍️ Думаю...", cancellationToken: ct);

  try
  {
    var reply = await AskGroqAsync(messageText, ct);
    await bot.SendTextMessageAsync(chatId, reply ?? "❌ Ошибка от Groq", cancellationToken: ct);
  }
  catch (Exception ex)
  {
    Console.WriteLine("Ошибка: " + ex.Message);
    await bot.SendTextMessageAsync(chatId, "⚠️ Ошибка при ответе.", cancellationToken: ct);
  }
}

Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct)
{
  Console.WriteLine($"❌ Ошибка Telegram API: {ex.Message}");
  return Task.CompletedTask;
}

async Task<string?> AskGroqAsync(string prompt, CancellationToken ct)
{
  var http = new HttpClient();
  http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", groqApiKey);

  var body = new
  {
    model = groqModel,
    messages = new[] { new { role = "user", content = prompt } }
  };

  var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
  var response = await http.PostAsync(groqApiUrl, content, ct);

  var json = await response.Content.ReadAsStringAsync(ct);
  Console.WriteLine("📦 Ответ Groq:\n" + json);

  using var doc = JsonDocument.Parse(json);

  if (doc.RootElement.TryGetProperty("choices", out var choices))
  {
    return choices[0].GetProperty("message").GetProperty("content").GetString();
  }

  if (doc.RootElement.TryGetProperty("error", out var err))
  {
    return "❌ Groq: " + err.GetProperty("message").GetString();
  }

  return "❓ Неизвестный ответ от Groq.";
}
