University Notice Telegram Bot

This C# program creates a Telegram bot that notifies users about new notices posted on the university website. Users need to provide their Telegram API token and Telegram user ID in the Program.cs file. The bot retrieves notice information from the designated university notice board URL, sends messages to users' Telegram accounts for new notices, and stores previously detected notices in a local file to avoid duplicate notifications.

How It Works

Initialization: Users need to provide their Telegram API token and user ID in the Program.cs file.

Scraping Notices: The program scrapes the designated university notice board URL to retrieve new notices.

Notification: When a new notice is detected, the bot sends a message to the user's Telegram account.

Persistence: Detected notices are stored locally to avoid duplicate notifications.



Setup

Clone Repository: Clone this repository to your local machine.

Configure Telegram API: Open the Program.cs file and replace the token and chatId variables with your Telegram API token and user ID respectively.

Run the Program: Build and run the program. It will start monitoring the university notice board and notify you of new notices on your Telegram account.


Dependencies

HtmlAgilityPack

Newtonsoft.Json

System.Net.Http

System.Threading.Tasks

System.IO

