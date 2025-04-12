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
  private readonly TelegramBotClient _bot;
  private readonly HttpClient _http = new();
  private const string GroqApiKey = "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"; // <-- Вставь свой ключ
  private const string GroqModel = "llama3-70b-8192";
  private const string GroqUrl = "https://api.groq.com/openai/v1/chat/completions";

  public TelegramController(TelegramBotClient bot)
  {
    _bot = bot;
    _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GroqApiKey);
  }

  [HttpPost]
  public async Task<IActionResult> Post([FromBody] Update update)
  {
    if (update.Type != UpdateType.Message || update.Message?.Text is not { } text)
      return Ok();

    var chatId = update.Message.Chat.Id;

    var requestBody = new
    {
      model = GroqModel,
      messages = new[] { new { role = "user", content = text } }
    };

    var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
    var response = await _http.PostAsync(GroqUrl, content);
    var json = await response.Content.ReadAsStringAsync();

    string reply = "⚠️ Ошибка.";
    using var doc = JsonDocument.Parse(json);
    if (doc.RootElement.TryGetProperty("choices", out var choices))
    {
      reply = choices[0].GetProperty("message").GetProperty("content").GetString() ?? reply;
    }

    await _bot.SendTextMessageAsync(chatId, reply);
    return Ok();
  }
}

