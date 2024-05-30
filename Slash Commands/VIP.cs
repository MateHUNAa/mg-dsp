using DiscordBotTemplateNet7.Utility;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;


namespace DiscordBotTemplateNet7.Slash_Commands
{
    public class VIP : ApplicationCommandModule
    {
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
                    DateTime userExpire = await GetUserExpirationDate(user.Id);
                    if (userExpire != DateTime.MinValue)
                    {
                        await RevokeVIP(guild, user, role, userExpire);
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
                Console.WriteLine(ex.ToString());
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

            var (hasDangerous, dangerousPermission) = util.HasDangerousPermissions(vipRole);
            if (hasDangerous)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Rime VIP",
                    Description = $"The givven VIP Role [{vipRole.Mention}] has Dangerous Permissions ! [{dangerousPermission}]",
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

            DiscordRole supporterRole = guild.Roles.Values.FirstOrDefault(r => r.Name.Equals("Tamogato", StringComparison.OrdinalIgnoreCase));

            if (supporterRole == null)
            {
                supporterRole = await guild.CreateRoleAsync("Tamogato", Permissions.None, DiscordColor.Orange, reason: "VIP: Supporter role was needed !");
            }

            var (supporter_hasDangerous, supporter_dangerousPermissions) = util.HasDangerousPermissions(supporterRole);
            if (supporter_hasDangerous)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Rime VIP",
                    Description = $"The givven VIP Role [{supporterRole.Mention}] has Dangerous Permissions ! [{supporter_dangerousPermissions}]",
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
                await member.GrantRoleAsync(supporterRole);
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

        public async Task<DateTime> GetUserExpirationDate(ulong userId)
        {
            var dbm = Program.dbManager;
            DateTime expirationDate = DateTime.MinValue;

            try
            {
                await dbm.OpenConnectionAsync();
                MySqlCommand cmd = dbm.CreateCommand("SELECT experiation_date FROM `mate_vipsystem` WHERE discordId = @did");
                cmd.Parameters.AddWithValue("@did", userId);

                using (MySqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    while (await reader.ReadAsync())
                    {
                        expirationDate = reader.GetDateTime("experiation_date");
                    }
                }
            } catch (Exception ex)
            {
                await Program._logger.LogErrorAsync(ex);
            } finally
            {

                await dbm.CloseConnectionAsync();
            }
            return expirationDate;
        }

        public async Task<bool> RevokeVIP(DiscordGuild guild, DiscordUser user, DiscordRole role, DateTime expirateionDate)
        {
            var dbm = Program.dbManager;
            DiscordEmbedBuilder embed;
            try
            {
                await dbm.OpenConnectionAsync();
                MySqlCommand cmd = dbm.CreateCommand("DELETE FROM `mate_vipsystem` WHERE discordId = @did");
                cmd.Parameters.AddWithValue("@did", user.Id);
                await cmd.ExecuteNonQueryAsync();

                embed = new DiscordEmbedBuilder
                {
                    Title = "Rime VIP",
                    Description = $"Revoked VIP from [{user.Mention}]! Original ExpireDate = [{expirateionDate}] Reason = VIP role was deleted !",
                    Color = DiscordColor.Aquamarine,
                    Timestamp = DateTime.UtcNow,
                };
                DiscordChannel channel = guild.Channels.Values.FirstOrDefault(ch => ch.Name.Equals("revoked-vips", StringComparison.OrdinalIgnoreCase)) ??
                                 await guild.CreateChannelAsync("revoked-vips", ChannelType.Text);

                await channel.SendMessageAsync(embed: embed);
                return true;
            } catch (Exception ex)
            {
                await Program._logger.LogErrorAsync(ex);
                return false;
            } finally
            {

                await dbm.CloseConnectionAsync();
            }
        }
    }
}
