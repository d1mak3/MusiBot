using Discord;

namespace MusiBotProd.Utilities.Visuals.DiscordButtons
{
    /// <summary>
    /// Blackjack stand button
    /// </summary>
    public class DiscordBlackjackStandButton : DiscordBlackjackButton
    {
        #region constructors

        public DiscordBlackjackStandButton() 
            : base(label: "Stand", customId: "blackjack-standed", style: ButtonStyle.Danger)
        {
            
        }

        #endregion

        #region button functions

        public override Task ExecuteAsync()
        {
            if (MessageComponent == null)
            {
                LogError("Error: Message of hit button was deleted");

                return Task.CompletedTask;
            }

            int userHand = 0, dealerHand = 0, betCoins = 0;
            string userName = "", guildName = "";

            string message = MessageComponent.Message.Content;
            message = message.Replace("\n", " ");

            string[] splittedMessage = message.Split(" ");

            int i = 0;

            while (splittedMessage[i] != "hand:")
            {
                userName += splittedMessage[i] + " ";
                ++i;
            }
            userName = userName.Trim();

            foreach (string msg in splittedMessage)
            {
                if (userHand == 0)
                {
                    try
                    {
                        userHand = Int32.Parse(msg);
                    }
                    catch
                    { }

                    continue;
                }

                if (dealerHand == 0)
                {
                    try
                    {
                        dealerHand = Int32.Parse(msg);
                    }
                    catch
                    { }

                    continue;
                }

                if (betCoins == 0)
                {
                    try
                    {
                        betCoins = Int32.Parse(msg);
                        break;
                    }
                    catch
                    { }
                }
            }

            i = splittedMessage.Length - 1;

            while (!splittedMessage[i].Contains("Guild:"))
            {
                guildName += splittedMessage[i] + " ";
                i--;
            }

            splittedMessage = guildName.Split(" ");
            Array.Reverse(splittedMessage);
            guildName = string.Join(" ", splittedMessage).Trim();

            int newCardValue = Random.Shared.Next(1, 10);

            if (newCardValue == 1 && dealerHand + 11 <= 21)
            {
                newCardValue = 11;
            }

            while (dealerHand + newCardValue <= 21)
            {
                dealerHand += newCardValue;
                newCardValue = Random.Shared.Next(1, 10);
            }

            if (dealerHand < userHand)
            {
                GiveCoinsToUser(userName, guildName, betCoins * 2);

                MessageComponent.RespondAsync($"Dealer busted! {userName} won {betCoins * 2}");
                MessageComponent.Message.DeleteAsync();

                return Task.CompletedTask;
            }

            if (dealerHand > userHand)
            {
                MessageComponent.RespondAsync($"Dealer got {dealerHand}\n" +
                        $"{userName} got {userHand}!\n{userName} lost {betCoins}");
                MessageComponent.Message.DeleteAsync();

                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        #endregion

        #region supporting methonds

        private void LogError(string error)
        {
            string logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
            string logFile = Path.Combine(logDirectory, $"{DateTime.UtcNow.ToString("yyyy-MM-dd")}.txt");

            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);
            if (!File.Exists(logFile))
                File.Create(logFile).Dispose();

            string logText = $"{DateTime.UtcNow.ToString("hh:mm:ss")} {error}";
            File.AppendAllText(logFile, logText + "\n");
        }

        #endregion
    }
}
