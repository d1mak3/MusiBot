using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MusiBotProd.Utilities.Data;
using MusiBotProd.Utilities.Data.DatabaseControllers;
using MusiBotProd.Utilities.Games;

namespace MusiBot.Modules
{
    [Name("Coi")]
    [RequireContext(ContextType.Guild)]
    public class DiscordCoiModule : ModuleBase<SocketCommandContext>
    {
        #region readonlies        

        private readonly IDataProvider _dataProvider;
        private readonly DatabaseController databaseController;
        private readonly ICoiGame _coiGame;

        #endregion

        #region constants

        private const double TIMESPAN_FOR_EARN = 15.0;
        private const double TIMESPAN_FOR_INCOME = 12 * 60;
        private const int MIN_EARN = 50;
        private const int MAX_EARN = 100;

        #endregion

        #region constructors

        public DiscordCoiModule(IDataProvider dataProvider, ICoiGame coiGame)
        {
            _dataProvider = dataProvider;
            databaseController = new DatabaseController(_dataProvider);
            _coiGame = coiGame;
        }

        #endregion

        #region commands

        [Command("earn")]
        [Alias("e")]
        [Summary("Earn some coins!")]
        public async Task EarnCoins()
        {
            string result = _coiGame.EarnCoins(Context.User.Id, Context.Guild.Id);

            await ReplyAsync(result);
        }

        // TODO: Add a func to see other users balances
        [Command("balance")]
        [Alias("bal")]
        [Summary("Check your balance!")]
        public async Task CheckBalance()
        {
            string result = _coiGame.CheckBalance(Context.User.Id, Context.Guild.Id);

            await ReplyAsync(result);
        }

        [Command("add_role_income")]
        [Alias("ari")]
        [Summary("Add an income value for a role!")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddIncome(string roleName, int income)
        {
            string result = _coiGame.AddIncome(Context.Guild.Id, roleName, income);

            await ReplyAsync(result);
        }

        [Command("check_roles_income")]
        [Alias("cri")]
        [Summary("Check an income value for roles on this server!")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task CheckIncomes()
        {
            string result = _coiGame.CheckIncomes(Context.Guild.Id);

            await ReplyAsync(result);
        }

        [Command("role_income")]
        [Alias("collect", "ri")]
        [Summary("Get an income for the roles you have on this server!")]
        public async Task GetIncomes()
        {
            string result = _coiGame.GetIncomes(Context.User.Id, Context.Guild.Id);

            await ReplyAsync(result);
        }

        [Command("on_deposit")]
        [Alias("dep", "od")]
        [Summary("Put your coins on deposit!")]
        public async Task PutOnDeposit(int value)
        {
            string result = _coiGame.PutOnDeposit(Context.User.Id, Context.Guild.Id, value);

            await ReplyAsync(result);
        }

        [Command("on_deposit")]
        [Alias("dep", "od")]
        [Summary("Put your coins on deposit!")]
        public async Task PutOnDeposit(string value)
        {
            string result = _coiGame.PutOnDeposit(Context.User.Id, Context.Guild.Id, value);

            await ReplyAsync(result);
        }

        [Command("in_cash")]
        [Alias("with", "cash", "ic")]
        [Summary("Take your coins from deposit!")]
        public async Task TakeFromDeposit(int value)
        {
            string result = _coiGame.TakeFromDeposit(Context.User.Id, Context.Guild.Id, value);

            await ReplyAsync(result);
        }

        [Command("in_cash")]
        [Alias("with", "cash", "ic")]
        [Summary("Take your coins from deposit!")]
        public async Task TakeFromDeposit(string value)
        {
            string result = _coiGame.TakeFromDeposit(Context.User.Id, Context.Guild.Id, value);

            await ReplyAsync(result);
        }

        [Command("fuck_your_coins")]
        [Alias("fyc", "delete_coins", "delc")]
        [Summary("Someone's coins have been vanished lmao!")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task VanishCoins(SocketGuildUser user)
        {
            string result = _coiGame.VanishCoins(user.Id, Context.Guild.Id);

            await ReplyAsync(result);
        }

        [Command("add_coins")]
        [Alias("ac", "addc")]
        [Summary("Add coins to someone!")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddCoins(SocketGuildUser user, int value)
        {
            string result = _coiGame.AddCoins(user.Id, Context.Guild.Id, value);

            await ReplyAsync(result);
        }

        [Command("rob")]
        [Alias()]
        [Summary("Rob someone!")]
        public async Task Rob(SocketGuildUser user)
        {
            string result = _coiGame.Rob(senderId: Context.User.Id, userToRobId: user.Id, Context.Guild.Id);

            await ReplyAsync(result);
        }

        [Command("roulette")]
        [Alias("rlte", "rt")]
        [Summary("Play roulette!")]
        public async Task PlayRoulette(int betNumber, int betCoins)
        {
            string result = _coiGame.PlayRoulette(Context.User.Id, Context.Guild.Id, betNumber, betCoins);

            await ReplyAsync(result);
        }

        [Command("roulette")]
        [Alias("rlte", "rt")]
        [Summary("Play roulette!")]
        public async Task PlayRoulette(int betNumber, string value)
        {
            string result = _coiGame.PlayRoulette(Context.User.Id, Context.Guild.Id, betNumber, value);

            await ReplyAsync(result);
        }

        [Command("roulette")]
        [Alias("rlte", "rt")]
        [Summary("Play roulette!")]
        public async Task PlayRoulette(string betColour, int betCoins)
        {
            string result = _coiGame.PlayRoulette(Context.User.Id, Context.Guild.Id, betColour, betCoins);

            await ReplyAsync(result);
        }

        [Command("roulette")]
        [Alias("rlte", "rt")]
        [Summary("Play roulette!")]
        public async Task PlayRoulette(string betColour, string value)
        {
            string result = _coiGame.PlayRoulette(Context.User.Id, Context.Guild.Id, betColour, value);

            await ReplyAsync(result);
        }

        [Command("leaderboard")]
        [Alias("lb")]
        [Summary("Check this server's leaderboard!")]
        public async Task CheckLeaderboard()
        {
            string result = _coiGame.CheckLeaderboard(Context.User.Id, Context.Guild.Id);

            await ReplyAsync(result);
        }

        [Command("leaderboard")]
        [Alias("lb")]
        [Summary("Check this server's leaderboard!")]
        public async Task CheckLeaderboard(string option)
        {
            string result = _coiGame.CheckLeaderboard(Context.User.Id, Context.Guild.Id, option);

            await ReplyAsync(result);
        }

        [Command("blackjack")]
        [Alias("bj")]
        [Summary("Play blackjack!")]
        public async Task PlayBlackjack(int betCoins)
        {
            object[] result = _coiGame.PlayBlackJack(Context.User.Id, Context.Guild.Id, betCoins);

            string message = result[0].ToString();

            if (result[1] is not MessageComponent buttons)
            {
                await ReplyAsync("Error: Something went wrong!");
                return;
            }

            await ReplyAsync(message, components: buttons);
        }

        [Command("blackjack")]
        [Alias("bj")]
        [Summary("Play blackjack!")]
        public async Task PlayBlackjack(string value)
        {
            object[] result = _coiGame.PlayBlackJack(Context.User.Id, Context.Guild.Id, value);

            string message = result[0].ToString();

            if (result[1] is not MessageComponent buttons)
            {
                await ReplyAsync("Error: Something went wrong!");
                return;
            }

            await ReplyAsync(message, components: buttons);
        }

        #endregion
    }
}