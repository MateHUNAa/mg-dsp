using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotTemplateNet7.Utility
{
    public class Logger
    {
        private DiscordClient _client;
        private DiscordChannel _logChannel;
        private DiscordChannel _errorLog;
        private DiscordChannel _joinLog;

        public Logger(DiscordClient client)
        {
            _client = client;

            InitializeLogChannelAsync().GetAwaiter().GetResult();

            ConsoleColors.WriteLineWithColors("[ ^4Logger ^0] [ ^2Info ^0] Logger Initialized.");
        }

        private async Task InitializeLogChannelAsync()
        {
            ulong logChannelId = 1194008427198943342;
            ulong logErrorId = 1194010406532939796;
            ulong joinLogId = 1194011404362055780;

            _logChannel = await _client.GetChannelAsync(logChannelId) as DiscordChannel;
            _errorLog = await _client.GetChannelAsync(logErrorId) as DiscordChannel;
            _joinLog = await _client.GetChannelAsync(joinLogId) as DiscordChannel;

            if (_logChannel == null || _errorLog == null || _joinLog == null)
            {
                var notFoundChannels = new List<string>();

                if (_logChannel == null)
                {
                    notFoundChannels.Add($"Log channel with ID {logChannelId}");
                }

                if (_errorLog == null)
                {
                    notFoundChannels.Add($"Error log channel with ID {logErrorId}");
                }

                if (_joinLog == null)
                {
                    notFoundChannels.Add($"Error log channel with ID {joinLogId}");
                }

                ConsoleColors.WriteLineWithColors("[ ^4Logger ^0] [ ^1Error ^0] Error: The following channel(s) were not found: ");
                foreach (var channel in notFoundChannels)
                {
                    ConsoleColors.WriteLineWithColors($"[ ^4Logger ^0] [ ^1Error ^0] - {channel}");
                }
            }

        }

        public async Task LogAsync(string message)
        {
            if (_logChannel != null)
            {
                await _logChannel.SendMessageAsync(message);
            } else
            {
                ConsoleColors.WriteLineWithColors("[ ^4Logger ^0] [ ^1Error ^0] Log channel not initialized.");
            }
        }

        public async Task LogErrorAsync(Exception ex)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Error Log",
                Description = ex.Message,
                Color = DiscordColor.Red
            };

            embed.AddField("StackTrace", $"```{ex.StackTrace}```");

            await _logChannel.SendMessageAsync(embed: embed);
            await _errorLog.SendMessageAsync(embed: embed);
        }


        public async Task LogJoinAsync(DiscordGuild guild)
        {
            if (_joinLog != null && _logChannel != null)
            {
                var joinEmbed = new DiscordEmbedBuilder
                {
                    Title = "Guild Joined",
                    Description = $"Joined guild: {guild.Name}",
                    Color = DiscordColor.Green
                };

                joinEmbed.AddField("Owner", guild.Owner.Username, inline: true);
                joinEmbed.AddField("Member Count", guild.MemberCount.ToString(), inline: true);
                joinEmbed.AddField("Verification Level", guild.VerificationLevel.ToString(), inline: true);
                joinEmbed.AddField("Created At", guild.CreationTimestamp.ToString("dd/MM/yyyy HH:mm:ss"), inline: false);

                // Send the joinEmbed to the log channel
                await _logChannel.SendMessageAsync(embed: joinEmbed);

                // Create a new embed instance for the join log channel
                var joinLogEmbed = new DiscordEmbedBuilder
                {
                    Title = "Guild Joined - Summary",
                    Description = $"Joined guild: {guild.Name} | Owner: {guild.Owner.Username}",
                    Color = DiscordColor.Green
                };

                joinLogEmbed.AddField("Guild ID", guild.Id.ToString());
                joinLogEmbed.AddField("Member Count", guild.MemberCount.ToString());
                joinLogEmbed.AddField("Created At", guild.CreationTimestamp.ToString("dd/MM/yyyy HH:mm:ss"));

                // Send the joinLogEmbed to the join log channel
                await _joinLog.SendMessageAsync(embed: joinLogEmbed);
                await _logChannel.SendMessageAsync(embed: joinEmbed);
            } else
            {
                ConsoleColors.WriteLineWithColors("[ ^4Logger ^0] [ ^1Error ^0] Join log channel not initialized.");
            }
        }



    }
}
