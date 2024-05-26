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
using System.Text;

class Program
{
    private static TelegramBotClient botClient;
    public static String last_command ="non";
    public static string logFilePath = "system_log.txt";
    public static string token = "7166228483:AAGD2P3z0o004YCT9jPMTz_EogX3zBcMEo8";
    public static string admin_id = "1274939394";
   // public static List<string> chatIds = new List<string> { "1274939394", "1892288693"};

 private static void Log(string message)
    {
        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            writer.WriteLine($"[{DateTime.Now}] {message}");
        }
    }
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
        Log("Program started. : ");
    Task.Run(() => start_bot());

    Task.Run(async () =>
    {
        DateTime lastChecked = DateTime.Now;

        while (true)
        {
            lastChecked = DateTime.Now;
            //Console.WriteLine($"Running... Last checked: {lastChecked}");
            Log($"Running... Last checked: {lastChecked}");
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
        Log($"Incoming update: {update.Type}");
    if (update.Type == UpdateType.Message)
    {
        if (update.Message.Type == MessageType.Text)
        {
            var text = update.Message.Text;
            var id = update.Message.Chat.Id;
            string? username = update.Message.Chat.Username;
            
            Console.WriteLine($"{username} | {id} | {text}");
            SaveResponse(update.Message);
           
            if(last_command=="feedback"){
                SaveFeedback(update.Message);
                last_command ="non";
                await bot.SendTextMessageAsync(update.Message.Chat.Id, "Thank you for your feedback!");
            }else{
            if (text.StartsWith("/start",StringComparison.OrdinalIgnoreCase) || text.StartsWith("/addme",StringComparison.OrdinalIgnoreCase))
            {
                
                if (!UserExists(id, username))
                {
                    AddUser(id,username);
                    await bot.SendTextMessageAsync(update.Message.Chat.Id, "Welcome to our bot! \nYou have been added! \n Here are the available commands:\n/help - See all the available shortcuts.\n/start - Start the bot.\n/addme - Start receiving notices.\n/last - Get the last notice from the website.\n/stop - Stop receiving notices.\n/feedback - Give feedback.\nFeel free to explore these commands and let us know if you have any questions or feedback!");
                }else{
                    await bot.SendTextMessageAsync(update.Message.Chat.Id, "You are already added! \n Here are the available commands:\n/help - See all the available shortcuts.\n/start - Start the bot.\n/addme - Start receiving notices.\n/last - Get the last notice from the website.\n/stop - Stop receiving notices.\n/feedback - Give feedback.\nFeel free to explore these commands and let us know if you have any questions or feedback!");
                
                }
                
            }else if (text.StartsWith("/stop", StringComparison.OrdinalIgnoreCase))
            {
                RemoveUser(id);
                await bot.SendTextMessageAsync(update.Message.Chat.Id, "You have been removed!");
            }else if (text.StartsWith("/feedback", StringComparison.OrdinalIgnoreCase))
            {
                //SaveFeedback(update.Message);
                last_command ="feedback";
                await bot.SendTextMessageAsync(update.Message.Chat.Id, "Type Your feedback or report ");
            }
            else if (text.StartsWith("/last", StringComparison.OrdinalIgnoreCase))
            {
                string lastNotice = GetLastNotice();
                await bot.SendTextMessageAsync(update.Message.Chat.Id, "https://www.aiub.edu"+lastNotice);
            }
            else if (text.StartsWith("/help", StringComparison.OrdinalIgnoreCase))
            {
               await bot.SendTextMessageAsync(update.Message.Chat.Id, "Here are the available commands:\n/help - See all the available shortcuts.\n/start - Start the bot.\n/addme - Start receiving notices.\n/last - Get the last notice from the website.\n/stop - Stop receiving notices.\n/feedback - Give feedback.\nFeel free to explore these commands and let us know if you have any questions or feedback!");
            }
            else if (id.ToString() == admin_id && text.StartsWith("/logs", StringComparison.OrdinalIgnoreCase))
            {
                // Send the log file to the admin
                //await SendLogFile(id);
            }
            else{
                await bot.SendTextMessageAsync(update.Message.Chat.Id, "Here are the available commands:\n/help - See all the available shortcuts.\n/start - Start the bot.\n/addme - Start receiving notices.\n/last - Get the last notice from the website.\n/stop - Stop receiving notices.\n/feedback - Give feedback.\nFeel free to explore these commands and let us know if you have any questions or feedback!");
            }
        }
            
        }
    }
    }
/*
private static async Task SendLogFile(long adminId)
{
    // Check if the log file exists
    if (System.IO.File.Exists(logFilePath))
    {
        // Read the log file content
        string logContent = System.IO.File.ReadAllText(logFilePath);

        // Convert log content to bytes
        byte[] logBytes = Encoding.UTF8.GetBytes(logContent);

        // Create an InputFile object from the log content bytes
         Telegram.Bot.Types.InputFile file = new Telegram.Bot.Types.File(new MemoryStream(logBytes), logFilePath);


        // Send the log file to the admin
        await botClient.SendTextMessageAsync(adminId, "Here is the log file:", parseMode: ParseMode.Markdown);
        await botClient.SendDocumentAsync(adminId, file, logFilePath, disableNotification: true);
    }
    else
    {
        // Log file doesn't exist
        await botClient.SendTextMessageAsync(adminId, "The log file is not available.");
    }
}*/
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
    private static string GetLastNotice()
{
    string filePath = "Notice.txt";
    
    if (System.IO.File.Exists(filePath))
    {
        try
        {
            // Read the file using explicit encoding (UTF-8)
            var lines = System.IO.File.ReadAllLines(filePath, Encoding.UTF8);
            
            // Check if there are any lines
            if (lines.Length > 0)
            {
                // Return the first notice
                return lines[0];
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while reading the file: {ex.Message}");
        }
    }
    
    return "No notices available.";
}
private static void SaveResponse(Message message)
{
    string response = message.Text;
    using (StreamWriter writer = new StreamWriter("response.txt", true))
    {
        writer.WriteLine($"ID: {message.Chat.Id} | Date: {DateTime.Now.Date.ToShortDateString()} | Time: {DateTime.Now.ToShortTimeString()}");
        writer.WriteLine($"Response: {response}");
        writer.WriteLine();
    }
}
private static void SaveFeedback(Message message)
{
    string feedback = message.Text.Replace("/feedback", "").Trim();
    using (StreamWriter writer = new StreamWriter("report.txt", true))
    {
        writer.WriteLine($"ID: {message.Chat.Id} | Date: {DateTime.Now.Date.ToShortDateString()} | Time: {DateTime.Now.ToShortTimeString()}");
        writer.WriteLine($"Report: {feedback}");
        writer.WriteLine();
    }
}
   private static void RemoveUser(long id)
{
    string[] existingUsers = System.IO.File.ReadAllLines("ids.txt");
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

internal class InputOnlineFile
{
}