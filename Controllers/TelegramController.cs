using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("webhook")]
public class TelegramController : ControllerBase
{
  private const string TelegramToken = "7899253021:AAEj4L2EIjIpZ4e2o941gjhoUSve17tynto";
  private const string GroqApiKey = "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";
  private const string GroqModel = "llama3-70b-8192";
  private const string GroqUrl = "https://api.groq.com/openai/v1/chat/completions";

  private readonly TelegramBotClient _botClient = new(TelegramToken);

  [HttpPost]
  public async Task<IActionResult> Post([FromBody] Update update)
  {
    if (update.Type != UpdateType.Message || update.Message?.Text == null)
      return Ok();

    var chatId = update.Message.Chat.Id;
    var userMessage = update.Message.Text;

    var reply = await AskGroqAsync(userMessage);
    await _botClient.SendTextMessageAsync(chatId, reply ?? "❌ Ошибка от Groq");

    return Ok();
  }

  private static async Task<string?> AskGroqAsync(string prompt)
  {
    using var http = new HttpClient();
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GroqApiKey);

    var body = new
    {
      model = GroqModel,
      messages = new[] { new { role = "user", content = prompt } }
    };

    var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
    var response = await http.PostAsync(GroqUrl, content);

    if (!response.IsSuccessStatusCode)
      return $"❌ Ошибка Groq API: {response.StatusCode}";

    var json = await response.Content.ReadAsStringAsync();

    using var doc = JsonDocument.Parse(json);
    if (doc.RootElement.TryGetProperty("choices", out var choices))
    {
      return choices[0].GetProperty("message").GetProperty("content").GetString();
    }

    if (doc.RootElement.TryGetProperty("error", out var error))
    {
      return $"❌ Groq: {error.GetProperty("message").GetString()}";
    }

    return "❓ Неизвестный ответ от Groq.";
  }
}
