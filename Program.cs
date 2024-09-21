using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using System.Net;
using System.Web;
using System.Text.Json;
using System.Text;


var botToken = "7525344033:AAFONNIYy0Gik572d4YN5EedWU5KDIi-4dU";
var CMCToken = "467a0080-282e-4fda-a118-8d6c89371915";
var bot = new TelegramBotClient(botToken);

var receiverOptions = new ReceiverOptions {
    AllowedUpdates = [UpdateType.Message]
};

bot.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions);
Console.ReadLine();

async Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken token)
{
    Console.WriteLine(exception.Message);
}

async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
{
    if (update.Message != null && update.Message.Type == MessageType.Text) {
        var text = update.Message.Text;
        var chat = update.Message.Chat;

        if (text == "/start") {
            await bot.SendTextMessageAsync(chat, "Привет! Используй команду /currency \"тикер токена\", чтобы узнать текущую цену токена. \nПример: /currency btc");
        }
        else if (text.Split(" ")[0] == "/currency") {
            if (text.Split(" ").Length == 1) {
                await bot.SendTextMessageAsync(chat, "Вы не указали тикер токена.");
            } else if (text.Split(" ").Length == 2) {
                var tokenName = text.Split(" ")[1];
                var price = makeAPICall(tokenName);
                if (price != null) {
                    var stringBuilder = new StringBuilder("Цена " + tokenName.ToUpper() + ": ");
                    stringBuilder.Append(price + "$");
                    await bot.SendTextMessageAsync(chat, stringBuilder.ToString());
                }
                else {
                    await bot.SendTextMessageAsync(chat, "Монета с данным тикером не найдена/ошибка.");
                }
            } else {
                await bot.SendTextMessageAsync(chat, "Неверный ввод.");
            }
        }
        else {
            await bot.SendTextMessageAsync(chat, "Неверный ввод. Доступные команды: \n/start \n/currency \"тикер токена\" (Пример: /currency btc)");
        }
    }
}

string? makeAPICall(string tokenName) {
    var URL = new UriBuilder("https://pro-api.coinmarketcap.com/v2/tools/price-conversion");

    var queryString = HttpUtility.ParseQueryString(string.Empty);
    queryString["amount"] = "1";
    queryString["symbol"] = tokenName.ToUpper();
    queryString["convert"] = "USD";

    URL.Query = queryString.ToString();

    var client = new WebClient();
    client.Headers.Add("X-CMC_PRO_API_KEY", CMCToken);
    client.Headers.Add("Accepts", "application/json");

    try {
        var jsonString = client.DownloadString(URL.ToString());
        var json = JsonDocument.Parse(jsonString).RootElement;
        return json.GetProperty("data")[0]
                   .GetProperty("quote")
                   .GetProperty("USD")
                   .GetProperty("price")
                   .ToString();
    }
    catch (KeyNotFoundException ex) {
        return null;
    }
    catch (WebException ex) {
        return null;
    }
}

