using Discord;

namespace MusiBotProd.Utilities.Visuals.DiscordButtons
{
    /// <summary>
    /// Blackjack hit button 
    /// </summary>
    public class DiscordBlackjackHitButton : DiscordBlackjackButton
    {
        #region constructors

        public DiscordBlackjackHitButton() 
            : base(label: "Hit", customId: "blackjack-hit", style: ButtonStyle.Success)
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

            if (newCardValue == 1 && userHand + 11 <= 21)
            {
                newCardValue = 11;
            }

            userHand += newCardValue;

            if (userHand == 21)
            {
                GiveCoinsToUser(userName, guildName, betCoins * 2);

                MessageComponent.RespondAsync($"Blackjack! {userName} won {betCoins * 2}");
                MessageComponent.Message.DeleteAsync();                

                return Task.CompletedTask;
            }

            if (userHand > 21)
            {
                MessageComponent.RespondAsync($"Busted! {userName} lost {betCoins}");
                MessageComponent.Message.DeleteAsync();

                return Task.CompletedTask;
            }

            MessageComponent.UpdateAsync(message => message.Content = $"{userName} hand: {userHand}\n" +
                $"Dealers hand: {dealerHand}\nBet is: {betCoins}\nGuild: {guildName}");

            return Task.CompletedTask;
        }

        #endregion

        #region supporting methods

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