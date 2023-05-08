using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace MusiBotProd.Services.DiscordServices
{
    /// <summary>
    /// Service to log events
    /// </summary>

    public class DiscordLoggingService
    {
        #region readonlies

        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;

        #endregion

        #region constructors

        /// <summary>
        /// Auto-completed by dependency injection constructor 
        /// </summary>
        public DiscordLoggingService(DiscordSocketClient discord, CommandService commands)
        {
            _discord = discord;
            _commands = commands;

            _discord.Log += OnLogAsync;
            _commands.Log += OnLogAsync;
        }

        #endregion

        #region event handlers

        /// <summary>
        /// Write log in file, when discords logs smth
        /// </summary>
        private Task OnLogAsync(LogMessage msg)
        {
            string logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
            string logFile = Path.Combine(logDirectory, $"{DateTime.UtcNow.ToString("yyyy-MM-dd")}.txt");

            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);
            if (!File.Exists(logFile))
                File.Create(logFile).Dispose();

            string logText = $"{DateTime.UtcNow.ToString("hh:mm:ss")} [{msg.Severity}] {msg.Source}: {msg.Exception?.ToString() ?? msg.Message}";
            File.AppendAllText(logFile, logText + "\n");

            return Console.Out.WriteLineAsync(logText);
        }

        #endregion
    }
}
