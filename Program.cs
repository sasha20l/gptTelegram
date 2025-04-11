using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

const string apiKey = "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"; // 🔐 вставь сюда токен с console.groq.com
const string model = "llama3-70b-8192";
const string baseUrl = "https://api.groq.com/openai/v1/chat/completions";

var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

Console.WriteLine("🚀 Groq (Mixtral) готов! Напиши что-нибудь ('exit' для выхода):");

while (true)
{
  Console.Write("> ");
  var userInput = Console.ReadLine();

  if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
    break;

  var requestBody = new
  {
    model,
    messages = new[]
      {
            new { role = "user", content = userInput }
        }
  };

  var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

  var response = await httpClient.PostAsync(baseUrl, content);
  var json = await response.Content.ReadAsStringAsync();

  using var doc = JsonDocument.Parse(json);

  if (doc.RootElement.TryGetProperty("choices", out var choices))
  {
    var reply = choices[0].GetProperty("message").GetProperty("content").GetString();
    Console.WriteLine($"\n🤖 Groq: {reply}\n");
  }
  else if (doc.RootElement.TryGetProperty("error", out var error))
  {
    var message = error.GetProperty("message").GetString();
    Console.WriteLine($"\n❌ Ошибка от Groq API: {message}");
  }
  else
  {
    Console.WriteLine("❓ Неизвестный ответ от Groq:\n" + json);
  }
}
