namespace MusiBot
{
    /// <summary>
    /// Main class which contains method which starts the bot
    /// </summary>
    public class Program
    {
        #region main method

        /// <summary>
        /// Main method which starts the bot
        /// </summary>        
        public static Task Main(string[] args) => Startup.ExecuteAsync();

        #endregion
    }
}
