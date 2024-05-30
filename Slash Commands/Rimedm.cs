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




        [SlashCommand("createVip", "Creates a VIP role !")]
        public async Task CreateVIPRole(InteractionContext context,
                                        [Option("roleName", "VIP Role name")] string roleName,
                                        [Option("Level", "Level for the VIP role")] long level)
        {
            DiscordGuild guild = context.Guild;
            DiscordEmbedBuilder embed;
            DiscordInteractionResponseBuilder responseBuilder = new DiscordInteractionResponseBuilder();

            if ((int)level > 20)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Rime VIP",
                    Description = "Level cannot be higher than 20 !",
                    Color = DiscordColor.IndianRed
                };
                responseBuilder.AddEmbed(embed);
                await context.CreateResponseAsync(responseBuilder);
            }

            var dbm = Program.dbManager;
            try
            {
                await dbm.OpenConnectionAsync();
                DiscordRole vipRole = await guild.CreateRoleAsync(roleName, Permissions.None, DiscordColor.Gold, false, false, "Createing a VIP Role !", null, null);

                MySqlCommand cmd = dbm.CreateCommand("INSERT INTO `vip_roles` (guildId, roleId, level, roleName) VALUES (@guildId, @roleId, @level, @roleName)");
                cmd.Parameters.AddWithValue("@guildId", guild.Id);
                cmd.Parameters.AddWithValue("@roleId", vipRole.Id);
                cmd.Parameters.AddWithValue("@level", level);
                cmd.Parameters.AddWithValue("@roleName", roleName);

                cmd.ExecuteNonQuery();
                embed = new DiscordEmbedBuilder
                {
                    Title = "Rime VIP",
                    Description = "Successfully created a VIP role !",
                    Color = DiscordColor.Green,
                };
                responseBuilder.AddEmbed(embed);
                await context.CreateResponseAsync(responseBuilder);

            } catch (Exception ex)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Rime VIP",
                    Description = "An error occured while createing VIP role.",
                    Color = DiscordColor.IndianRed,
                };
                responseBuilder.AddEmbed(embed);
                await context.CreateResponseAsync(responseBuilder);
#if DEBUG 
                await Program._logger.LogErrorAsync(ex);
#endif
            } finally
            {
                await dbm.CloseConnectionAsync();
            }

        }

        [SlashCommand("delVIP", "Delete a VIP role !")]
        public async Task DeleteVIPRole(InteractionContext context,
                                        [Option("role", "Mention the role")] DiscordRole role)
        {
            DiscordEmbedBuilder embed;
            DiscordInteractionResponseBuilder responseBuilder = new DiscordInteractionResponseBuilder();
            DiscordGuild guild = context.Guild;

            var (isVip, vipLevel) = await isRoleVIP(role, guild.Id);
            if (!isVip)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Rime VIP",
                    Description = "The mentioned role is not a VIP role!",
                    Color = DiscordColor.IndianRed,
                };
                responseBuilder.AddEmbed(embed);
                await context.CreateResponseAsync(responseBuilder);
            }

            var dbm = Program.dbManager;

            DiscordUser[] users = await GetRoleUsers(role.Id, guild);

            if (users != null)
            {
                foreach (var user in users)
                {
                    try
                    {
                        DiscordMember member = await guild.GetMemberAsync(user.Id);
                        await member.RevokeRoleAsync(role);

                        await dbm.OpenConnectionAsync();
                        MySqlCommand cmd = dbm.CreateCommand("DELETE FROM `mate_vipsystem` WHERE discordId = @did");
                        cmd.Parameters.AddWithValue("@did", user.Id);
                        await cmd.ExecuteNonQueryAsync();

                        embed = new DiscordEmbedBuilder
                        {
                            Title = "Rime VIP",
                            Description = $"Revoked VIP from {user.Mention}! Reason = VIP Role was deleted !",
                            Color = DiscordColor.DarkRed,
                            Timestamp = DateTime.UtcNow // or user-specific timestamp if needed
                        };

                        DiscordChannel channel = guild.Channels.Values.FirstOrDefault(ch => ch.Name.Equals("revoked-vips", StringComparison.OrdinalIgnoreCase));

                        if (channel == null)
                        {
                            channel = await guild.CreateChannelAsync("revoked-vips", ChannelType.Text);
                        }

                        await channel.SendMessageAsync(embed: embed);
                    } catch (Exception ex)
                    {
                        await Program._logger.LogErrorAsync(ex);
                    } finally
                    {
                        await dbm.CloseConnectionAsync();
                    }
                }
            }

            try
            {
                await dbm.OpenConnectionAsync();
                MySqlCommand cmd = dbm.CreateCommand("DELETE FROM `vip_roles` WHERE roleId = @roleId AND guildId = @guildId");
                cmd.Parameters.AddWithValue("@roleId", role.Id);
                cmd.Parameters.AddWithValue("@guildId", guild.Id);
                await cmd.ExecuteNonQueryAsync();

                await role.DeleteAsync("VIP: No longer needed !");

                embed = new DiscordEmbedBuilder
                {
                    Title = "Rime VIP",
                    Description = "Successfully removed VIP role !",
                    Color = DiscordColor.Green,
                };
                responseBuilder.AddEmbed(embed);
                await context.DeferAsync();
                await context.DeleteResponseAsync();

                await context.Channel.SendMessageAsync(embed: embed);

            } catch (Exception ex)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Rime VIP",
                    Description = "Something went wrong while deleteing VIP role.",
                    Color = DiscordColor.IndianRed
                };
                responseBuilder.AddEmbed(embed);
                await context.CreateResponseAsync(responseBuilder);
                await Program._logger.LogErrorAsync(ex);
            } finally
            {

                await dbm.CloseConnectionAsync();
            }
        }

        [SlashCommand("givevip", "Give VIP status for a player")]
        public async Task GiveVIPToUser(InteractionContext context,
                                        [Option("user", "New Vip user")] DiscordUser user,
                                        [Option("role", "VIP Role")] DiscordRole vipRole,
                                        [Option("expireDate", "Expiration Date")] string durationStr)
        {
            DiscordEmbedBuilder embed;
            DiscordInteractionResponseBuilder responseBuilder = new DiscordInteractionResponseBuilder();

            var dbm = Program.dbManager;
            var util = new utility();
            DateTimeOffset experiationDate;
            DiscordGuild guild = context.Guild;

            if (util.TryParseDuration(durationStr, out TimeSpan duration))
            {
                experiationDate = DateTimeOffset.UtcNow.Add(duration);
            } else
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Rime VIP",
                    Description = "Invalid duration format. Please provide the duration in a valid format (e.g., '5d' for 5 days)",
                    Color = DiscordColor.IndianRed,
                };
                responseBuilder.AddEmbed(embed);
                await context.CreateResponseAsync(responseBuilder);
                throw new ArgumentException("Invalid duration format. Please provide the duration in a valid format (e.g., '5d' for 5 days)");
            }


            var (isVip, viplevel) = await isRoleVIP(vipRole, guild.Id);

            if (!isVip)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Rime VIP",
                    Description = "The mentioned role is not a VIP role!",
                    Color = DiscordColor.IndianRed,
                };
                responseBuilder.AddEmbed(embed);
                await context.CreateResponseAsync(responseBuilder);
            }

            if (vipRole.Permissions == Permissions.Administrator || vipRole.Permissions == Permissions.ManageChannels || vipRole.Permissions == Permissions.ManageGuild || vipRole.Permissions == Permissions.ManageRoles || vipRole.Permissions == Permissions.KickMembers || vipRole.Permissions == Permissions.BanMembers ||
                vipRole.Permissions == Permissions.ViewAuditLog)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Rime VIP",
                    Description = "The givven VIP role has permissions ! Please remove all Permision",
                    Color = DiscordColor.IndianRed,
                };
                responseBuilder.AddEmbed(embed);
                await context.CreateResponseAsync(responseBuilder);
            }

            if (await isUserVIP(user))
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Rime VIP",
                    Description = "This user already a VIP member !",
                    Color = DiscordColor.IndianRed,
                };
                responseBuilder.AddEmbed(embed);
                await context.CreateResponseAsync(responseBuilder);
            }

            try
            {
                await dbm.OpenConnectionAsync();
                MySqlCommand cmd = dbm.CreateCommand("INSERT INTO `mate_vipsystem` (discordId, level, experiation_date, roleId) VALUES (@discordId, @level, @experiation_date, @roleId)");
                cmd.Parameters.AddWithValue("@level", viplevel);
                cmd.Parameters.AddWithValue("@discordId", user.Id);
                cmd.Parameters.AddWithValue("@roleId", vipRole.Id);

                Console.WriteLine(experiationDate);
                cmd.Parameters.AddWithValue("@experiation_date", experiationDate);

                await cmd.ExecuteNonQueryAsync();
                embed = new DiscordEmbedBuilder { Title = "Rime VIP", Description = $"Successfully givven VIP to {user.Mention} duration: {experiationDate}", Color = DiscordColor.Green };

                DiscordMember member = await guild.GetMemberAsync(user.Id);
                await member.GrantRoleAsync(vipRole);
                // TODO: Give supporter role !
                responseBuilder.AddEmbed(embed);
                await context.CreateResponseAsync(responseBuilder);
            } catch (Exception ex)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Rime VIP",
                    Description = "Something went wrong while giving VIP",
                    Color = DiscordColor.IndianRed,
                };

                responseBuilder.AddEmbed(embed);
                await context.CreateResponseAsync(responseBuilder);
                Console.WriteLine(ex.ToString());
                await Program._logger.LogErrorAsync(ex);
            } finally
            {
                await dbm.CloseConnectionAsync();
            }
        }

        [SlashCommand("takevip", "Take the VIP status from user")]
        public async Task TakeVIPFromUser(InteractionContext context,
                                          [Option("user", "Mention a user")] DiscordUser user)
        {
            DiscordGuild guild = context.Guild;
            DiscordEmbedBuilder embed;
            DiscordInteractionResponseBuilder responseBuilder = new DiscordInteractionResponseBuilder();
            bool isVIP = await isUserVIP(user);

            if (!isVIP)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Rime VIP",
                    Description = "The mentioned user is not a VIP",
                    Color = DiscordColor.IndianRed,
                };
                responseBuilder.AddEmbed(embed);
                await context.CreateResponseAsync(responseBuilder);
            }
            DiscordRole vipRole = await GetUserVIPRole(user, guild);

            var dbm = Program.dbManager;
            try
            {
                await dbm.OpenConnectionAsync();
                MySqlCommand cmd = dbm.CreateCommand("DELETE FROM `mate_vipsystem` WHERE discordId = @discordId");
                cmd.Parameters.AddWithValue("@discordId", user.Id);
                await cmd.ExecuteNonQueryAsync();
                embed = new DiscordEmbedBuilder
                {
                    Title = "Rime VIP",
                    Description = $"Successfully taken VIP status from {user.Mention}",
                    Color = DiscordColor.Green,
                };

                DiscordMember member = await guild.GetMemberAsync(user.Id);

                if (vipRole != null)
                {
                    await member.RevokeRoleAsync(vipRole);
                } else
                {
                    responseBuilder.WithContent($"Failed to remove VIP role from {member.Mention}");
                }

                responseBuilder.AddEmbed(embed);
                await context.CreateResponseAsync(responseBuilder);

            } catch (Exception ex)
            {
                await Program._logger.LogErrorAsync(ex);
            } finally
            {
                await dbm.CloseConnectionAsync();
            }

        }


        public async Task<DiscordUser[]> GetRoleUsers(ulong roleId, DiscordGuild guild)
        {
            var dbm = Program.dbManager;
            try
            {
                await dbm.OpenConnectionAsync();
                List<DiscordUser> users = new List<DiscordUser>();

                using (var cmd = dbm.CreateCommand("SELECT discordId FROM `mate_vipsystem` WHERE roleId = @roleId"))
                {
                    cmd.Parameters.AddWithValue("@roleId", roleId);

                    using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        while (await reader.ReadAsync())
                        {
                            ulong discordId = (ulong)reader.GetInt64(0);
                            Console.WriteLine("DISCORDID: " + discordId.ToString());

                            var user = await guild.GetMemberAsync(discordId);
                            if (user != null)
                            {
                                Console.WriteLine($"ADDIG: {user.Username} TO THE LIST");
                                users.Add(user);
                            }
                        }
                    }
                }

                return users.ToArray();

            } catch (Exception ex)
            {
                await Program._logger.LogErrorAsync(ex);
                return null;
            } finally
            {
                await dbm.CloseConnectionAsync();
            }
        }


        public async Task<DiscordRole> GetUserVIPRole(DiscordUser user, DiscordGuild guild)
        {
            var dbm = Program.dbManager;
            try
            {
                await dbm.OpenConnectionAsync();

                MySqlCommand cmd = dbm.CreateCommand("SELECT roleId FROM `mate_vipsystem` WHERE discordId=@discordId");
                cmd.Parameters.AddWithValue("@discordId", user.Id);

                using (MySqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    while (await reader.ReadAsync())
                    {
                        string roleIdStr = reader.GetString("roleId");
                        Console.WriteLine("FASZ");
                        if (ulong.TryParse(roleIdStr, out ulong roleId))
                        {
                            return guild.GetRole(roleId);
                        } else
                        {
                            return null;
                        }
                    }
                    return null;
                }
            } catch (Exception ex)
            {
                await Program._logger.LogErrorAsync(ex);
                return null;
            } finally
            {

                await dbm.CloseConnectionAsync();
            }
        }

        public async Task<bool> isUserVIP(DiscordUser user)
        {
            var dbm = Program.dbManager;

            try
            {
                await dbm.OpenConnectionAsync();


                MySqlCommand cmd = new MySqlCommand("SELECT discordId FROM `mate_vipsystem` WHERE discordId = @discordId", dbm._connection);
                cmd.Parameters.AddWithValue("@discordId", user.Id);
                MySqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

                while (await reader.ReadAsync())
                {
                    return true;
                }

                await reader.CloseAsync();
                return false;
            } catch (DbException ex)
            {
                await Program._logger.LogErrorAsync(ex);
                return false;
            } finally
            {
                await dbm.CloseConnectionAsync();
            }
        }

        public async Task<(bool isVip, int vipLevel)> isRoleVIP(DiscordRole role, ulong guildId)
        {

            var dbm = Program.dbManager;

            try
            {
                await dbm.OpenConnectionAsync();

                var parameters = new Dictionary<string, object>
                {
                    {"@guildId", guildId },
                    {"@roleId", role.Id},
                };
                using var reader = await dbm.ExecuteReaderWithParametersAsync("SELECT roleId, level FROM `vip_roles` WHERE guildId = @guildId AND roleId = @roleId", parameters);

                while (await reader.ReadAsync())
                {
                    int vipLevel = reader.GetInt32("level");
                    return (true, vipLevel);
                }
                await reader.CloseAsync();
            } catch (Exception ex)
            {
                await Program._logger.LogErrorAsync(ex);
            } finally
            {
                await dbm.CloseConnectionAsync();
            }

            return (false, 0);
        }
    }
}
