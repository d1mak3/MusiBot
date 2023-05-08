using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace MusiBotProd.Modules.DiscordModules
{
    [Name("Moderation")]
    [RequireContext(ContextType.Guild)]
    public class DiscordModeratorModule : ModuleBase<SocketCommandContext>
    {
        #region readonlies

        private readonly Dictionary<string, ulong> _channelAlias = new Dictionary<string, ulong>()
        {
            { "gaming", 988518680252010496 },
            { "flood", 988516628243296297 },
            { "anime", 988521402208509952 },
            { "afk", 476410927374139394 },
            { "hidden", 476146163343556618 },
            { "нудистский пляж", 460893065679470596 }
        };

        #endregion

        #region commands

        [Command("move")]
        [Alias("mv")]
        [Summary("Move user to other channel!")]
        [RequireUserPermission(GuildPermission.MoveMembers)]
        [RequireBotPermission(GuildPermission.MoveMembers)]
        public async Task MoveUser(SocketGuildUser user, string channelName)
        {
            if (user == null)
            {
                await ReplyAsync("User not found!");
                return;
            }

            SocketChannel? channel = _channelAlias.ContainsKey(channelName.ToLower()) && user.Guild.Id == 460893065679470592 ?
                user.Guild.GetVoiceChannel(_channelAlias[channelName.ToLower()]) : // if there is alias for the channel, get channel dict
                user.Guild.VoiceChannels.FirstOrDefault(ch => ch.Name == channelName); // else find by name         

            if (channel == null)
            {
                await ReplyAsync("Channel not found!");
                return;
            }

            if (user.VoiceChannel == null)
            {
                await ReplyAsync("User is not in a voice channel!");
                return;
            }

            await user.ModifyAsync(u => u.ChannelId = channel!.Id);
        }

        [Command("moveall")]
        [Alias("mvall")]
        [Summary("Move everyone from one channel to another!")]
        [RequireUserPermission(GuildPermission.MoveMembers)]
        [RequireBotPermission(GuildPermission.MoveMembers)]
        public async Task MoveAllUsers(string channelNameToMoveFrom, string channelNameToMoveTo)
        {
            SocketChannel? channelToMoveFrom = _channelAlias.ContainsKey(channelNameToMoveFrom.ToLower()) && Context.Guild.Id == 460893065679470592 ?
                Context.Guild.GetVoiceChannel(_channelAlias[channelNameToMoveFrom.ToLower()]) : // if there is alias for the channel, get channel dict
                Context.Guild.VoiceChannels.FirstOrDefault(ch => ch.Name == channelNameToMoveFrom); // else find by name

            SocketChannel? channelToMoveTo = _channelAlias.ContainsKey(channelNameToMoveTo.ToLower()) && Context.Guild.Id == 460893065679470592 ?
                Context.Guild.GetVoiceChannel(_channelAlias[channelNameToMoveTo.ToLower()]) :
                Context.Guild.VoiceChannels.FirstOrDefault(ch => ch.Name == channelNameToMoveTo);

            if (channelToMoveFrom == null)
            {
                await ReplyAsync("Channel to move from not found!");
                return;
            }

            if (channelToMoveTo == null)
            {
                await ReplyAsync("Channel to move to not found!");
                return;
            }

            foreach (SocketGuildUser user in (channelToMoveFrom as SocketVoiceChannel)!.ConnectedUsers)
            {
                await user.ModifyAsync(u => u.ChannelId = channelToMoveTo.Id);
            }
        }

        [Command("mute")]
        [Alias("mt")]
        [Summary("Mute user!")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task MuteUser(IGuildUser user)
        {
            try
            {
                await user.ModifyAsync(u => u.Mute = true);
            }
            catch
            {
                await Context.Channel.SendMessageAsync("User should be in a voice channel!");
            }
        }

        [Command("unmute")]
        [Alias("unmt")]
        [Summary("Unmute user!")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.Administrator)]
        public async Task UnmuteUser(IGuildUser user)
        {
            try
            {
                await user.ModifyAsync(u => u.Mute = false);
            }
            catch
            {
                await Context.Channel.SendMessageAsync("User should be in a voice channel!");
            }
        }

        #endregion
    }
}
