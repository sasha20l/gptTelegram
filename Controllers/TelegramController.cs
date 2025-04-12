using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;

[ApiController]
[Route("webhook")]
public class TelegramController : ControllerBase
{
  private readonly TelegramBotClient _bot;
  private readonly HttpClient _http = new();
  private readonly string _groqApiKey;

  private const string GroqModel = "llama3-70b-8192";
  private const string GroqUrl = "https://api.groq.com/openai/v1/chat/completions";

  public TelegramController(TelegramBotClient bot, IConfiguration config)
  {
    _bot = bot;
    _groqApiKey = config["Groq:ApiKey"];
    _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _groqApiKey);
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
