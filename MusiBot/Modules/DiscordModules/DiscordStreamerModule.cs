using Discord.Commands;
using Discord.WebSocket;
using System.Net;

namespace MusiBot.Modules
{
    [Name("Stream")]
    public class DiscordStreamerModule : ModuleBase<SocketCommandContext>
    {
        #region commands

        [Command("announce")]
        [Alias("an")]
        [Summary("Announce your stream!")]
        public async Task AnnounceStream()
        {
            switch (Context.User.Id)
            {
                case 350604575377719298: // Seva
                    string link = "https://www.twitch.tv/husevalen";

                    if (!CheckIfStreamerIsLive(link))
                    {
                        await ReplyAsync("Stream is offline!");
                        return;
                    }

                    SocketTextChannel? announcementChannel = Context.Guild.GetChannel(873526051853193296) as SocketTextChannel; // "announcements" channel
                    await Context.Message.DeleteAsync();
                    await announcementChannel!.SendMessageAsync(link);
                    break;

                case 441995512837832714: // Raf
                    link = "https://www.twitch.tv/d_raf_";

                    if (!CheckIfStreamerIsLive(link))
                    {
                        await ReplyAsync("Stream is offline!");
                        return;
                    }

                    announcementChannel = Context.Guild.GetChannel(873526051853193296) as SocketTextChannel; // "announcements" channel
                    await Context.Message.DeleteAsync();
                    await announcementChannel!.SendMessageAsync(link);
                    break;

                default:
                    await ReplyAsync("You are not a streamer!");
                    break;
            }   
        }

        [Command("announcewith")]
        [Alias("anw")]
        [Summary("Announce your stream with a description!")]
        public async Task AnnounceStream([Remainder] string streamDescription)
        {
            switch (Context.User.Id)
            {
                case 350604575377719298: // Seva
                    string link = "https://www.twitch.tv/husevalen";

                    if (!CheckIfStreamerIsLive(link))
                    {
                        await ReplyAsync("Stream is offline!");
                        return;
                    }

                    SocketTextChannel? announcementChannel = Context.Guild.GetChannel(873526051853193296) as SocketTextChannel; // "announcements" channel
                    await Context.Message.DeleteAsync();
                    await announcementChannel!.SendMessageAsync(link + " " + streamDescription);
                    break;

                case 441995512837832714: // Raf
                    link = "https://www.twitch.tv/d_raf_";

                    if (!CheckIfStreamerIsLive(link))
                    {
                        await ReplyAsync("Stream is offline!");
                        return;
                    }

                    announcementChannel = Context.Guild.GetChannel(873526051853193296) as SocketTextChannel; // "announcements" channel
                    await Context.Message.DeleteAsync();
                    await announcementChannel!.SendMessageAsync(link + " " + streamDescription);
                    break;

                default:
                    await ReplyAsync("You are not a streamer!");
                    break;
            }
        }

        #endregion

        #region supporting methods

        private bool CheckIfStreamerIsLive(string link)
        {
            var messageRequest = WebRequest.Create(link);
            string response;

            using (var reader = new StreamReader(messageRequest.GetResponse().GetResponseStream()))
            {
                response = reader.ReadToEnd();
            }

            return response.Contains("\"isLiveBroadcast\":true") ? true : false;
        }

        #endregion
    }
}
