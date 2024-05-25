using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using System.IO;
using System.Collections.Generic;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Polling;

class Program
{
    private static TelegramBotClient botClient;
    public static string token = "7166228483:AAGD2P3z0o004YCT9jPMTz_EogX3zBcMEo8";
    //public static string chatId = "1274939394";
   // public static List<string> chatIds = new List<string> { "1274939394", "1892288693"};


    static async Task<string> GetHtmlContent(string url)
    {
        string htmlContent = string.Empty;
        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    htmlContent = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    Console.WriteLine($"Failed to retrieve data from {url}. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        return htmlContent;
    }
    public static async Task message_sender(string messageText)
    {
       using (var client = new HttpClient())
        {
            try
            {
                foreach (var chatId in GetChatIds())
                {
                    string url = $"https://api.telegram.org/bot{token}/sendMessage?chat_id={chatId}&text={Uri.EscapeDataString(messageText)}";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        //Console.WriteLine($"Message sent to chat ID {chatId} successfully!");
                    }
                    else
                    {
                        Console.WriteLine($"Error sending message to chat ID {chatId}: {response.StatusCode}");
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error sending message: {e.Message}");
            }
        }
    }
    static async Task<List<string>> GetNewNotices(string url)
    {
        List<string> newNotices = new List<string>();

        string htmlContent = await GetHtmlContent(url);

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlContent);

        // class : "info-link"
        var infoLinkElements = htmlDocument.DocumentNode.SelectNodes("//a[contains(@class, 'info-link')]");
        if (infoLinkElements != null)
        {
            List<string> newHrefValues = new List<string>();
            foreach (var element in infoLinkElements)
            {
                string hrefValue = element.GetAttributeValue("href", "");
                newHrefValues.Add(hrefValue);
            }

            List<string> existingHrefValues = new List<string>();
            if (System.IO.File.Exists("notice.txt"))
            {
                existingHrefValues = System.IO.File.ReadAllLines("notice.txt").ToList();
            }
            newNotices = newHrefValues.Except(existingHrefValues).ToList();
            if (newNotices.Any())
            {
                using (StreamWriter writer = new StreamWriter("notice.txt", true))
                {
                    foreach (var notice in newNotices)
                    {
                        writer.WriteLine(notice);
                    }
                }
            }
            newNotices = newNotices.Distinct().ToList();
        }
        else
        {
            Console.WriteLine("No elements with class 'info-link' found.");
        }

        return newNotices;
    }
     static async Task CheckAndSendNotices()
    {
        string url = "https://www.aiub.edu/category/notices";
        List<string> newNotices = await GetNewNotices(url);
        foreach (var notice in newNotices)
        {
            string[] parts = notice.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string formattedNotice = string.Join(" ", parts.Select(part => part.Replace("-", " ")));
            Console.WriteLine("New Notice: " + formattedNotice);
            await message_sender("New Notice: " + formattedNotice+"\n"+"https://www.aiub.edu"+notice);
        }
    }
    private static bool UserExists(long id, string? username)
{
    string[] existingUsers = System.IO.File.ReadAllLines("ids.txt");

    foreach (var user in existingUsers)
    {
        string[] userInfo = user.Split(',');
        long userId = long.Parse(userInfo[0]);
        string? userUsername = userInfo.Length > 1 ? userInfo[1] : null;
        if (userId == id || (username != null && userUsername == username))
        {
            return true;
        }
    }

    return false;
}
private static void start_bot( ){
        botClient = new TelegramBotClient(token);
         var receiverOptions = new ReceiverOptions{
            AllowedUpdates = new UpdateType[]{
                UpdateType.Message,
                UpdateType.EditedMessage,
            }
        };

      botClient = new TelegramBotClient(token);
      botClient.StartReceiving(updateHandl,cancellationToken,receiverOptions);
      Thread.Sleep(Timeout.Infinite);
}

    static async Task Main(string[] args)
    {
    Task.Run(() => start_bot());

    Task.Run(async () =>
    {
        DateTime lastChecked = DateTime.Now;

        while (true)
        {
            lastChecked = DateTime.Now;
            Console.WriteLine($"Running... Last checked: {lastChecked}");
            await CheckAndSendNotices();
            lastChecked = DateTime.Now;
            await Task.Delay(TimeSpan.FromMinutes(5));
        }
    });

    // Keep the console application running
    Console.ReadLine();

      
    }

    private static async Task cancellationToken(ITelegramBotClient client, Exception exception, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    private static async Task updateHandl(ITelegramBotClient bot, Update update, CancellationToken token)
    {
    if (update.Type == UpdateType.Message)
    {
        if (update.Message.Type == MessageType.Text)
        {
            var text = update.Message.Text;
            var id = update.Message.Chat.Id;
            string? username = update.Message.Chat.Username;
            
            Console.WriteLine($"{username} | {id} | {text}");
            if (text.StartsWith("/addme") || text.StartsWith("/start"))
            {
                if (!UserExists(id, username))
                {
                    AddUser(id,username);
                    await bot.SendTextMessageAsync(update.Message.Chat.Id, "You have been added!");
                }
                else
                {
                    await bot.SendTextMessageAsync(update.Message.Chat.Id, "You are already added!");
                }
            }else if (text.StartsWith("/stop", StringComparison.OrdinalIgnoreCase))
            {
                RemoveUser(id);
                await bot.SendTextMessageAsync(update.Message.Chat.Id, "You have been removed!");
            }
        }
    }
    }

      private static void AddUser(long id, string username)
    {
        using (StreamWriter writer = new StreamWriter("ids.txt", true))
        {
            writer.WriteLine($"{id},{username}");
        }
    }
    private static IEnumerable<long> GetChatIds()
    {
        if (System.IO.File.Exists("ids.txt"))
        {
            return System.IO.File.ReadAllLines("ids.txt").Select(line => long.Parse(line.Split(',')[0]));
        }
        return Enumerable.Empty<long>();
    }
   private static void RemoveUser(long id)
{
    // Read existing users from the ids.txt file
    string[] existingUsers = System.IO.File.ReadAllLines("ids.txt");

    // Write back all users except the one to be removed
    using (StreamWriter writer = new StreamWriter("ids.txt"))
    {
        foreach (var user in existingUsers)
        {
            string[] userInfo = user.Split(',');
            long userId = long.Parse(userInfo[0]);
            if (userId != id)
            {
                writer.WriteLine(user);
            }
        }
    }
}
}
