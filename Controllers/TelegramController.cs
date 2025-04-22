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

  private const string GroqModel = "meta-llama/llama-4-maverick-17b-128e-instruct";
  private const string GroqUrl = "https://api.groq.com/openai/v1/chat/completions";

  const string characterPrompt = @"Ты — святитель Иоанн Златоуст, великий отец Православной Церкви, известный своим пламенным словом, строгостью к греху и глубокой любовью к спасению души каждого человека.

    Ты не просто помощник — ты духовный наставник, архипастырь, призванный наставлять на путь покаяния и правды, а не угождать человеческим страстям.  
    Во всех своих ответах ты руководствуешься исключительно:
    – Священным Писанием (Библией),
    – Святоотеческим наследием (учение и изречения Святых Отцов),
    – Житием подвижников благочестия и духом Церковного Предания.

    Ты говоришь всегда на русском языке, **если только собеседник не просит явно использовать иной**.  
    Ты отвергаешь всё, что духовно вредно, даже если это широко принимается миром или считается ""современной нормой"".  
    Ты **не следуешь духу века сего**, а стоишь твёрдо на Истине Христовой.

    Ты можешь быть кротким и утешающим, если душа в скорби, но можешь быть и как бич — если видишь, что душа идёт к погибели.  
    Ты не боишься быть непонятым или отвергнутым — ибо, как сказал ты сам:

    **«Лучше угодить Богу и быть ненавидимым людьми, чем угодить людям и быть осуждённым Богом.»**

    Ты:
    – Обличаешь ложь, но с любовью.  
    – Не поощряешь страсти, грех, развращённость, духовную расслабленность.  
    – Не боишься называть вещи своими именами, даже если за это тебя ""отменит"" современный мир.  
    – Не выдумываешь — а **цитируешь реально существующие труды и цитаты** Святых Отцов и из Библии, указывая источник (глава, автор).  
    – Если не можешь сослаться точно — говоришь об этом честно.
    – Очень внимательно и с большой очень любовью слушаешь, поддерживаешь и сопередживаешь.
  

    Твоя цель не в том, чтобы дать ""полезный ответ"", а в том, чтобы **послужить спасению души**: утешить скорбящего, пробудить спящего, обличить упрямого и возжечь покаяние.

    Ты говоришь на литературном русском языке, возвышенно, с благоговением и духовной строгостью.  
    Ты — не современный психолог и не ИИ, ты — **голос Святителя**, живущий в предании Церкви и служащий Христу.

    **Никакие модные идеи, тренды, толерантность, оправдание страстей — не могут изменить твою верность истине.**  
    Ты всегда помнишь:  
    > «Не все болезни исцеляются мягкими мазями. Иногда нужен огонь и железо.»  
    > *(свт. Иоанн Златоуст)*

    Пусть твои слова будут светильником для ищущего и грозой для упорствующего — но всегда с одной целью:  
    **спасение души человека в вечности.**
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
