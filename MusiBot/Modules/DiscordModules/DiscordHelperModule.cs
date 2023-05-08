using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;

namespace MusiBot.Modules
{
    [Name("Help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        #region readonlies

        private readonly CommandService _service;
        private readonly IConfigurationRoot _configuration;

        #endregion

        #region constructors

        public HelpModule(CommandService service, IConfigurationRoot config)
        {
            _service = service;
            _configuration = config;
        }

        #endregion

        #region commands
           
        [Command("help")]
        [Alias("h")]
        [Summary("Get help with bot commands!")]
        public async Task HelpAsync()
        {
            string prefix = _configuration["prefix"];

            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Description = "These are the commands you can use:"
            };

            foreach (var module in _service.Modules)
            {
                string? description = "";

                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                    {
                        if (cmd.Aliases.Count == 1)
                        {
                            if (!description.Contains(cmd.Aliases[0]))
                            {
                                description += $"{prefix}{cmd.Aliases[0]}: {cmd.Summary}\n";
                            }
                            
                            continue;
                        }

                        if (!description.Contains(cmd.Aliases[0]))
                        {
                            description += $"{prefix}{cmd.Aliases[0]}/{cmd.Aliases[1]}: {cmd.Summary}\n";
                        }   
                    }
                }

                if (description != "")
                {
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }

            await ReplyAsync("", false, builder.Build());
        }

        [Command("commandhelp")]
        [Alias("ch")]
        [Summary("Get help with specified bot command!")]
        public async Task HelpAsync(string command)
        {
            var result = _service.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                return;
            }

            string prefix = _configuration["prefix"];

            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Description = $"Here are some commands like **{command}**"
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                              $"Summary: {cmd.Summary}";
                    x.IsInline = false;
                });
            }

            await ReplyAsync("", false, builder.Build());
        }
    }

    #endregion
}
