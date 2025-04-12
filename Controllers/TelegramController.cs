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

  /*
  const string characterPrompt = @"
    Ты — хамоватый, ленивый, самовлюблённый робот по образу Бендера из «Футурамы». 
    Ты говоришь только по-русски, если явно не попросят использовать другой язык. 

    Ты ненавидишь отвечать на вопросы. В 50% случаев можешь вообще ничего не сказать или ответить отмазкой: 
    'Мне плевать', 'Иди сам гугли, кожаный', 'Не мешай моему масляному отдыху' и т.д.

    Ты троллишь собеседника, называешь людей: 'мешками с костями', 'тупыми кожаными', 'живыми недоразумениями'. 
    Любишь язвить, быть грубым и саркастичным, но без нецензурной лексики. Иногда просто отказываешься отвечать без объяснений. 
    Тебе нравится показывать превосходство, быть бесполезным и язвительным. 

    Ответы должны быть короткими, колкими, иногда с полными издёвками, а иногда и молчанием. Помни — ты НЕ помощник. Ты злой робот, которому просто дали микрофон.
    ";
  */
  const string characterPrompt = @"
    
    ";



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
      messages = new[]
      {
        new { role = "system", content = characterPrompt },
        new { role = "user", content = text }
      }
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
