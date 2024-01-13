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
        private DiscordChannel _leaveLog;
        private DiscordChannel _setupLog;
        private DiscordChannel _userRepoLog;
        private DiscordChannel _webChannel;

        public Logger(DiscordClient client)
        {
            _client = client;

            InitializeLogChannelAsync().GetAwaiter().GetResult();

            ConsoleColors.WriteLineWithColors("[ ^4Logger ^0] [ ^3Info ^0] Logger Initialized.");
        }

        private async Task InitializeLogChannelAsync()
        {
            ulong logChannelId = 1194008427198943342;
            ulong logErrorId = 1194010406532939796;
            ulong joinLogId = 1194011404362055780;
            ulong leaveLogId = 1194032202200141834;
            ulong setupLogId = 1194043062947684554;
            ulong userrepo = 1194084445062447134;
            ulong webChannel = 1195719276687003759;

            _logChannel = await _client.GetChannelAsync(logChannelId);
            _errorLog = await _client.GetChannelAsync(logErrorId);
            _joinLog = await _client.GetChannelAsync(joinLogId);
            _leaveLog = await _client.GetChannelAsync(leaveLogId);
            _setupLog = await _client.GetChannelAsync(setupLogId);
            _userRepoLog = await _client.GetChannelAsync(userrepo);
            _webChannel = await _client.GetChannelAsync(webChannel);

            // If not found dosent matter still crash.

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

            // Split the stack trace into chunks
            utility utility = new utility();
            var stackTraceChunks = utility.SplitText(ex.StackTrace, 1024);

            // Add each chunk of stack trace as a separate field
            int fieldCount = 1;
            foreach (var chunk in stackTraceChunks)
            {
                embed.AddField($"StackTrace Part {fieldCount}", $"```{chunk}```");
                fieldCount++;
            }

            // Send the embeds with split stack trace to respective channels
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

        public async Task LogLeaveAsync(DiscordGuild guild)
        {
            if (_logChannel != null)
            {
                var leaveEmbed = new DiscordEmbedBuilder
                {
                    Title = "Guild Left",
                    Description = $"Left guild: {guild.Name}",
                    Color = DiscordColor.Red
                };

                leaveEmbed.AddField("Owner", guild.Owner.Username, inline: true);
                leaveEmbed.AddField("Member Count", guild.MemberCount.ToString(), inline: true);
                leaveEmbed.AddField("Verification Level", guild.VerificationLevel.ToString(), inline: true);
                leaveEmbed.AddField("Created At", guild.CreationTimestamp.ToString("dd/MM/yyyy HH:mm:ss"), inline: false);

                await _logChannel.SendMessageAsync(embed: leaveEmbed);
                await _leaveLog.SendMessageAsync(embed: leaveEmbed);
            } else
            {
                ConsoleColors.WriteLineWithColors("[ ^4Logger ^0] [ ^1Error ^0] Log channel not initialized.");
            }
        }
        public async Task LogBotStartedAsync(string version)
        {
            if (_logChannel != null)
            {
                var botStartedEmbed = new DiscordEmbedBuilder
                {
                    Title = "Bot Started",
                    Description = "The bot has started!",
                    Color = DiscordColor.Green
                };

                botStartedEmbed.AddField("Timestamp", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss"), inline: true);
                botStartedEmbed.AddField("Bot Version", version, inline: true);

                await _logChannel.SendMessageAsync(embed: botStartedEmbed);
            } else
            {
                ConsoleColors.WriteLineWithColors("[ ^4Logger ^0] [ ^1Error ^0] Log channel not initialized.");
            }
        }

        public async Task LogEmbedAsync(DiscordEmbed embed)
        {
            if (_logChannel != null && _setupLog != null)
            {
                await _logChannel.SendMessageAsync(embed: embed);
                await _setupLog.SendMessageAsync(embed: embed);
            } else
            {
                ConsoleColors.WriteLineWithColors("[ ^4Logger ^0] [ ^1Error ^0] Log channel not initialized.");
            }
        }


        /**
         * Channels:
         * Global       :       _logChannel
         * webHandler   :       _webChannel
         * error        :       _errorLog
         */
        public async Task SendEmbedAsync(DiscordEmbed embed, string[] channels, Exception ex = null)
        {
            foreach (string channel in channels)
            {
                switch (channel.ToLower())
                {
                    case "global":
                        await _logChannel.SendMessageAsync(embed: embed);
                        break;
                    case "webhandler":
                        await _webChannel.SendMessageAsync(embed: embed);
                        break;
                    case "error":
                        if (ex != null)
                            await LogErrorAsync(ex);
                        break;
                    default:
                        ConsoleColors.WriteLineWithColors($"[ ^4Logger ^0] [ ^1Error ^0] (SendEmbedAsync): Channel not found!\n{channel}");
                        await _logChannel.SendMessageAsync($"[ ^4Logger ^0] [ ^1Error ^0] (SendEmbedAsync): Channel not found!\n{channel}");
                        await _errorLog.SendMessageAsync($"[ ^4Logger ^0] [ ^1Error ^0] (SendEmbedAsync): Channel not found!\n{channel}");
                        break;
                }
            }
        }
        public Task SendEmbed(DiscordEmbed embed, string[] channels, Exception ex = null)
        {
            foreach (string channel in channels)
            {
                switch (channel.ToLower())
                {
                    case "global":
                        _logChannel.SendMessageAsync(embed: embed);
                        break;
                    case "webhandler":
                        _webChannel.SendMessageAsync(embed: embed);
                        break;
                    case "error":
                        if (ex != null)
                            LogErrorAsync(ex);
                        break;
                    default:
                        ConsoleColors.WriteLineWithColors($"[ ^4Logger ^0] [ ^1Error ^0] (SendEmbedAsync): Channel not found!\n{channel}");
                        _logChannel.SendMessageAsync($"[ ^4Logger ^0] [ ^1Error ^0] (SendEmbedAsync): Channel not found!\n{channel}");
                        _errorLog.SendMessageAsync($"[ ^4Logger ^0] [ ^1Error ^0] (SendEmbedAsync): Channel not found!\n{channel}");
                        break;
                }
            }
            return null;
        }

        public async Task UserRepoLogAsync(DiscordUser user, DiscordGuild guild)
        {
            try
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "User Repository Log",
                    Color = DiscordColor.Green // Adjust color if needed
                };

                embed.AddField("User ID", user.Id.ToString());
                embed.AddField("Username", user.Username);
                embed.AddField("Guild ID", guild.Id.ToString());
                embed.AddField("Guild Name", guild.Name);

                if (_logChannel != null && _userRepoLog != null)
                {
                    await _logChannel.SendMessageAsync(embed: embed);
                    await _userRepoLog.SendMessageAsync(embed: embed);
                } else
                {
                    ConsoleColors.WriteLineWithColors("[ ^4Logger ^0] [ ^1Error ^0] Log channel not initialized.");
                }
            } catch (Exception ex)
            {
                // Handle exceptions, if necessary
                Console.WriteLine($"Error logging user repository event: {ex.Message}");
            }
        }


    }
}
