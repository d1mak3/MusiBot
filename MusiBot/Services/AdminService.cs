using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace MusiBot.Services
{
    /// <summary>
    /// Service to manage admin things
    /// </summary>
    public class AdminService
    {
        #region readonlies

        private readonly DiscordSocketClient _discordClient;        
        private readonly IConfigurationRoot _configuration;        

        private readonly Dictionary<string, string> _emotesToRoles = new Dictionary<string, string>()
        {
            { "Valorant", "Valoranter" },
            { "ApexLegends", "Apex Legend" },
            { "TNT", "TNT Runner" },
            { "Minecraft", "Minecraft Enjoyer" },
            { "Genshin", "Genshin Impacter" },
            { "Dota2", "Cocksucker Dotaplayer" },
            { "Hehe", "Anime Bro" },
            { "MonkaS", "Metal Fan" },
            { "EliteDanya", "Hip Hop Head" },
            { "DBD", "Dead by Daylight" }
        };

        #endregion

        #region constructors

        /// <summary>
        /// Auto-completed by dependency injection constructor 
        /// </summary>
        public AdminService(DiscordSocketClient discordClient, IConfigurationRoot configuration)
        {
            _discordClient = discordClient;            
            _configuration = configuration;            

            _discordClient.ReactionAdded += OnRoleReactionAdded;
            _discordClient.ReactionRemoved += OnRoleReactionRemoved;
            _discordClient.UserJoined += OnUserJoined;            
        }

        #endregion

        #region event handlers

        /// <summary>
        /// Grant role to a user, when he added a reaction to the role message
        /// </summary>        
        private async Task OnRoleReactionAdded(Cacheable<IUserMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> channel,
            SocketReaction reaction)
        {
            if (reaction == null) return;

            if (reaction.MessageId != Convert.ToUInt64(_configuration["rolesmessageid"])) return;

            SocketRole role = GetRoleByReaction(reaction);            

            await (reaction.User.Value as IGuildUser)!.AddRoleAsync(role);
        }

        /// <summary>
        /// Remove a role from user, when he removed a reaction from the role message
        /// </summary> 
        private async Task OnRoleReactionRemoved(Cacheable<IUserMessage, ulong> message,
            Cacheable<IMessageChannel, ulong> channel,
            SocketReaction reaction)
        {
            if (reaction == null) return;

            if (reaction.MessageId != Convert.ToUInt64(_configuration["rolesmessageid"])) return;

            SocketRole role = GetRoleByReaction(reaction);

            if (!(reaction.User.Value as IGuildUser)!.RoleIds.Any(r => r == role.Id)) return;

            await (reaction.User.Value as IGuildUser)!.RemoveRoleAsync(role);
        }

        /// <summary>
        /// Give user a role, when he joined
        /// </summary>        
        private async Task OnUserJoined(SocketGuildUser user)
        {
            if (user.IsBot) return;

            await user.AddRoleAsync(
                    _discordClient.GetGuild(Convert.ToUInt64(_configuration["myguildid"]))
                        .Roles.FirstOrDefault(r => r.Name == "Cool guy"));
        }

        #endregion

        #region supporting methods

        /// <summary>
        /// Compare role with reaction
        /// </summary>
        private SocketRole GetRoleByReaction(SocketReaction reaction)
        {
            string roleName = _emotesToRoles[reaction.Emote.Name];                     

            return _discordClient
                .GetGuild(Convert.ToUInt64(_configuration["myguildid"]))
                .Roles.FirstOrDefault(r => r.Name == roleName);
        }

        #endregion
    }
}
