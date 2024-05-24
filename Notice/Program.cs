using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.IO;

class Program
{
    public static string token = "7166228483:AAGD2P3z0o004YCT9jPMTz_EogX3zBcMEo8";
    public static string chatId = "1274939394";

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
        string url = $"https://api.telegram.org/bot{token}/sendMessage?chat_id={chatId}&text={Uri.EscapeDataString(messageText)}";
        using (var client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                   // Console.WriteLine("Message sent successfully!");
                }
                else
                {
                    Console.WriteLine($"Error sending message: {response.StatusCode}");
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
            if (File.Exists("notice.txt"))
            {
                existingHrefValues = File.ReadAllLines("notice.txt").ToList();
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
    static async Task Main(string[] args)
    {
       
       string url = "https://www.aiub.edu/category/notices";
List<string> newNotices = await GetNewNotices(url);
foreach (var notice in newNotices)
{
    string[] parts = notice.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
    string formattedNotice = string.Join(" ", parts.Select(part => part.Replace("-", " ")));
    Console.WriteLine("New Notice: " + formattedNotice);
    await message_sender("New Notice: " + formattedNotice);
}
    }
}
