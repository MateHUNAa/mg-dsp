using DiscordBotTemplateNet7.Utility;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MySql.Data.MySqlClient;
using Mysqlx.Cursor;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.Sig;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Macs;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Runtime.Serialization;

namespace DiscordBotTemplateNet7.Slash_Commands
{
    public class Rimedm : ApplicationCommandModule
    {

        [SlashCommand("leaderboard", "Getting the current leaderboard")]
        public async Task Leaderboard(InteractionContext context,
                                [Option("type", "<kills, kd, faction>")] string type,
                                [Option("page", "Page number")] long page = 1,
                                [Option("entries", "Number of entries per page")] long entriesPerPage = 10)
        {
            DiscordEmbedBuilder embed;
            DiscordInteractionResponseBuilder responseBuilder = new DiscordInteractionResponseBuilder();
            var db = Program.dbManager;
            string cmd = "";

            int offset = ((int)page - 1) * (int)entriesPerPage;

            try
            {
                int totalCount;
                switch (type)
                {
                    case "kills":
                        totalCount = await GetTotalCountAsync("mate_kd");
                        break;
                    case "kd":
                        totalCount = await GetTotalCountAsync("mate_kd");
                        break;
                    case "faction":
                        totalCount = await GetTotalCountAsync("mate_factionkd");
                        break;
                    default:
                        totalCount = await GetTotalCountAsync("mate_kd");
                        break;
                }

                int totalPages = (int)Math.Ceiling((double)totalCount / entriesPerPage);

                page = Math.Max(1, Math.Min(page, totalPages));

                string title = $"RimeDM - Leaderboard (Page {page} of {totalPages})";

                switch (type)
                {
                    case "kills":
                        cmd = $"SELECT * FROM `mate_kd` ORDER BY kills DESC LIMIT {entriesPerPage} OFFSET {offset}";
                        break;
                    case "kd":
                        cmd = $"SELECT * FROM `mate_kd` ORDER BY kd DESC LIMIT {entriesPerPage} OFFSET {offset}";
                        break;
                    case "faction":
                        cmd = $"SELECT * FROM `mate_factionkd` ORDER BY kills DESC LIMIT {entriesPerPage} OFFSET {offset}";
                        break;
                    default:
                        cmd = $"SELECT * FROM `mate_kd` ORDER BY kills DESC LIMIT {entriesPerPage} OFFSET {offset}";
                        break;
                }

                await db.OpenConnectionAsync();
                using var reader = await db.ExecuteReaderAsync(cmd);

                var embedBuilder = new DiscordEmbedBuilder()
                    .WithTitle(title)
                    .WithColor(DiscordColor.Gold)
                    .WithFooter($"Made By: RimeDM Development team ❤")
                    .WithTimestamp(DateTime.UtcNow);


                int totalKills = 0;
                int totalDeaths = 0;
                int totalHeadshots = 0;
                while (await reader.ReadAsync())
                {
                    string _title;
                    string description;

                    if (type != "faction")
                    {
                        int kills = reader.GetInt32("kills");
                        int deaths = reader.GetInt32("deaths");
                        int headshot = reader.GetInt32("headshot");
                        totalKills += kills;
                        totalDeaths += deaths;
                        totalHeadshots += headshot;
                        string username = reader.GetString("player_name");

                        _title = $"User: {username}";
                        description = $"Kills: {kills}\nDeaths: {deaths}\nHeadshots: {headshot}";
                    } else
                    {
                        string faction_name = reader.GetString("job");
                        int kills = reader.GetInt32("kills");
                        int deaths = reader.GetInt32("deaths");
                        int headshots = reader.GetInt32("headshots");
                        totalKills += kills;
                        totalDeaths += deaths;
                        totalHeadshots += headshots;


                        _title = $"JOB: {faction_name}";
                        description = $"Kills: {kills}\nDeaths: {deaths}\nHeadshots: {headshots}";

                    }
                    embedBuilder.WithDescription($"**Total Kills:** {totalKills}\n**Total Deaths:** {totalDeaths}\n**Total Kills/Deaths:** {(totalKills / totalDeaths)}\n**Total Headshots/kills:** {(totalKills / totalHeadshots)}");
                    embedBuilder.AddField(_title, description);
                }


                var row = new DiscordComponent[]
                {
                  new DiscordButtonComponent(ButtonStyle.Primary, $"previus-page", "⬅"),
                  new DiscordButtonComponent(ButtonStyle.Primary, $"next-page", "➡"),
                };


                responseBuilder.AddEmbed(embed: embedBuilder.Build()).AddComponents(row);
                await context.CreateResponseAsync(responseBuilder);
            } catch (Exception ex)
            {
                await Program._logger.LogErrorAsync(ex);
            } finally
            {
                await db.CloseConnectionAsync();
            }
        }

        private async Task<int> GetTotalCountAsync(string tableName)
        {
            var db = Program.dbManager;
            string countCmd = $"SELECT COUNT(*) FROM `{tableName}`";
            try
            {
                await db.OpenConnectionAsync();
                using (var reader = await db.ExecuteReaderAsync(countCmd))
                {
                    if (reader != null && await reader.ReadAsync())
                    {
                        return reader.GetInt32(0);
                    } else
                    {
                        return 0;
                    }
                }
            } finally
            {
                await db.CloseConnectionAsync();
            }
        }


        [SlashCommand("setup-leaderboard", "Setup the leaderboard")]
        public async Task SetupLeaderboard(InteractionContext context,
                                      [Option("channel", "Select the channel to post the leaderboard in")] DiscordChannel channel,
                                      [Option("type", "Order by: kills, kd, faction")] string type)
        {
            var responseBuilder = new DiscordInteractionResponseBuilder();
            var dbm = Program.dbManager;
            DiscordEmbedBuilder embed;

            switch (type.ToLower())
            {
                case "kills":
                    embed = new DiscordEmbedBuilder
                    {
                        Title = "Leaderboard",
                        Color = DiscordColor.Gold,
                        Description = "Listing the top 10 players, sorted by kills"
                    };
                    break;
                case "kd":
                    embed = new DiscordEmbedBuilder
                    {
                        Title = "Leaderboard",
                        Color = DiscordColor.Gold,
                        Description = "Listing the top 10 players, sorted by KD ratio"
                    };
                    break;
                case "faction":
                    embed = new DiscordEmbedBuilder
                    {
                        Title = "Leaderboard",
                        Color = DiscordColor.Gold,
                        Description = "Listing the top 10 factions"
                    };
                    break;
                default:
                    embed = new DiscordEmbedBuilder
                    {
                        Title = "Leaderboard",
                        Color = DiscordColor.Gold,
                        Description = "Listing the top 10 players"
                    };
                    break;
            }

            try
            {
                await dbm.OpenConnectionAsync();

                var message = await channel.SendMessageAsync(embed: embed);
                var command = dbm.CreateCommand("INSERT INTO `leaderboard` (channelId, messageId, guildId, type) VALUES (@channelId, @messageId, @guildId, @type)");
                command.Parameters.AddWithValue("@channelId", channel.Id);
                command.Parameters.AddWithValue("@messageId", message.Id);
                command.Parameters.AddWithValue("@guildId", context.Guild.Id);
                command.Parameters.AddWithValue("@type", type);

                command.ExecuteNonQuery();

                embed = new DiscordEmbedBuilder
                {
                    Title = "Leaderboard",
                    Color = DiscordColor.Green,
                    Description = "You have successfully registered a leaderboard on this server!"
                };
            } catch (Exception ex)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Color = DiscordColor.IndianRed,
                    Description = "Something went wrong while creating the leaderboard."
                };
                responseBuilder.AddEmbed(embed);
                await context.CreateResponseAsync(responseBuilder);

                await Program._logger.LogErrorAsync(ex);

                return; // Ensure the method exits here in case of error
            } finally
            {
                await dbm.CloseConnectionAsync();
            }

            responseBuilder.AddEmbed(embed);
            await context.CreateResponseAsync(responseBuilder);
        }
    }
}
