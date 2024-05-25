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


class Program
{
    private static TelegramBotClient botClient;
    public static string token = "7166228483:AAGD2P3z0o004YCT9jPMTz_EogX3zBcMEo8";
    //public static string chatId = "1274939394";
    public static List<string> chatIds = new List<string> { "1274939394", "1892288693"};


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
                foreach (var chatId in chatIds)
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
    

    static async Task Main(string[] args)
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
    
     // botClient = new TelegramBotClient(token);
      
    }
}
