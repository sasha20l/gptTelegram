using Telegram.Bot;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson(); // <-- Добавляем NewtonsoftJson
builder.Services.AddSingleton(new TelegramBotClient("7899253021:AAGpvgkcLgQi3Fp5-5K942jylVi_Xt41oDI")); // <-- Вставь токен

var app = builder.Build();

app.UseRouting();
app.MapControllers();

app.Run();


