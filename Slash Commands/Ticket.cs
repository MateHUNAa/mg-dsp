using DiscordBotTemplateNet7.Repositories;
using DiscordBotTemplateNet7.Utility;
using DiscordBotTemplateNet7.Valami;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.VisualBasic;
using MySql.Data.MySqlClient;
using System;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;

namespace DiscordBotTemplateNet7.Slash_Commands
{
    public class Ticket : ApplicationCommandModule
    {

        [SlashCommand("remove", "Remove a user from a specific channel")]
        public async Task RemoveUserFromChannel(InteractionContext context, [Option("user", "The user to be removed")] DiscordUser user)
        {
            try
            {
                var guild = context.Guild;
                var channel = context.Channel;
                var member = await guild.GetMemberAsync(user.Id);

                if (member != null)
                {
                    await channel.AddOverwriteAsync(member, Permissions.None, Permissions.AccessChannels);

                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Removed {user.Mention} from {channel.Name}."));
                } else
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("User not found."));
                }
            } catch (Exception ex)
            {
                ConsoleColors.WriteLineWithColors($"[ ^4Ticket ^0] [ ^1Error ^0] Error removing ticket system from the guild: {ex.Message}");
                await Program._logger.LogErrorAsync(ex);
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("An error occurred."));
            }
        }

        [SlashCommand("add", "Add a user to a specific channel")]
        public async Task AddUserToChannel(InteractionContext context, [Option("user", "The user's Discord ID")] DiscordUser user)
        {
            try
            {
                var member = await context.Guild.GetMemberAsync(user.Id);

                if (member != null)
                {
                    DiscordChannel channel = context.Channel;
                    await channel.AddOverwriteAsync(member, Permissions.SendMessages, Permissions.None);
                    await channel.AddOverwriteAsync(member, Permissions.AccessChannels, Permissions.None);

                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Added <@{user.Mention}> to {channel.Name}."));
                } else
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("User not found."));
                }
            } catch (Exception ex)
            {
                ConsoleColors.WriteLineWithColors($"[ ^4Ticket ^0] [ ^1Error ^0] Error removing ticket system from the guild: {ex.Message}");
                await Program._logger.LogErrorAsync(ex);
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("An error occurred."));
            }
        }


        [SlashCommand("ticket-setup", "Setup a Ticket Channel")]
        [SlashRequireGuild]
        [RequirePermissions(Permissions.Administrator)]
        public async Task SetupTicketSystem(InteractionContext context,
                                            [Option("Channel", "The Channel to set up the ticket system in")] DiscordChannel channel,
                                            [Option("Category", "The Category where channels goes.")] DiscordChannel category)
        {
            DiscordInteractionResponseBuilder intbuilder = new DiscordInteractionResponseBuilder();
            var dbm = Program.dbManager;
            DiscordEmbedBuilder embed;
            if (category.Type != ChannelType.Category)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Ticket System 🎟",
                    Description = "The mentioned category is not a category! ❌",
                    Color = DiscordColor.IndianRed
                };
                DiscordInteractionResponseBuilder responseBuilder = new DiscordInteractionResponseBuilder();
                responseBuilder.AddEmbed(embed);
                await context.CreateResponseAsync(responseBuilder);
                return;
            }

            await dbm.OpenConnectionAsync();

            bool isTicket = await IsTicketSystemSetupAsync(context.Guild.Id);
            if (isTicket)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Ticket System",
                    Description = $"You already initialized a Ticket System!\nYou can remove the previus one /ticket-remove .",
                    Color = DiscordColor.IndianRed
                };
                intbuilder.AddEmbed(embed: embed);
                await context.CreateResponseAsync(intbuilder);
                return;
            }



            try
            {
                await dbm.OpenConnectionAsync();

                embed = new DiscordEmbedBuilder
                {
                    Title = "Ticket System",
                    Color = DiscordColor.Green,
                    Description = $"Ticket System initialized in {context.Guild.Name}",

                };

                embed.AddField("GuildId", context.Guild.Id.ToString());
                embed.AddField("GuildName", context.Guild.Name);
                embed.AddField("TotalMembers", context.Guild.MemberCount.ToString());


                await Program._logger.LogEmbedAsync(embed);

                embed = new DiscordEmbedBuilder
                {
                    Title = "Ticket System",
                    Color = DiscordColor.Green,
                    Description = "You created ticket system!"
                };
                intbuilder.AddEmbed(embed: embed);
                await context.CreateResponseAsync(intbuilder);

                embed = new DiscordEmbedBuilder
                {
                    Title = "🎟 Ticket System",
                    Description = "Welcome to the Ticket section. Please select how our support team can assist you. 📋",
                    Color = DiscordColor.Green
                };

                var row = new DiscordComponent[]
                    {
                    new DiscordButtonComponent(ButtonStyle.Primary, "tamogatas", "❤ Tamogatas"),
                    new DiscordButtonComponent(ButtonStyle.Primary, "bug", "🐛 BUG"),
                    new DiscordButtonComponent(ButtonStyle.Primary, "frakciok", "🛠 Frakciok"),
                    new DiscordButtonComponent(ButtonStyle.Primary, "admin_tgf", "🛡 Admin TGF")
                    };

                var builder = new DiscordMessageBuilder().WithEmbed(embed).AddComponents(row);
                DiscordMessage message = await channel.SendMessageAsync(builder);
                MySqlCommand command = dbm.CreateCommand("INSERT INTO `ticketsystem` (guildId, channelId, categoryId, MessageId)" +
                                                    "VALUES (@GuildId, @ChannelId, @CategoryId, @MessageId)");

                command.Parameters.AddWithValue("@GuildId", context.Guild.Id);
                command.Parameters.AddWithValue("@ChannelId", channel.Id);
                command.Parameters.AddWithValue("@CategoryId", category.Id);
                command.Parameters.AddWithValue("@MessageId", message.Id);
                command.ExecuteNonQuery();
            } catch (Exception ex)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Ticket System",
                    Color = DiscordColor.IndianRed,
                    Description = "Something went wrong while creating ticket system."
                };
                intbuilder.AddEmbed(embed: embed);
                await context.CreateResponseAsync(intbuilder);
                await Program._logger.LogErrorAsync(ex);
            } finally
            {
                dbm.CloseConnection();
            }


        }

        [SlashCommand("ticket-remove", "Remove Ticket System from server")]
        [SlashRequireGuild]
        [RequirePermissions(Permissions.Administrator)]
        public async Task RemoveTicket(InteractionContext context,
        [Option("guildid", "Guild ID (optional)")] string guildId = null)
        {
            DiscordEmbedBuilder embed;
            if (guildId != null)
            {
                DiscordGuild guild = context.Guild;
                int numberOfTickets = await DeleteTicketSystemFromGuildAsync(guild);
                embed = new DiscordEmbedBuilder
                {
                    Title = "Ticket System",
                    Color = DiscordColor.Green,
                    Description = $"Ticket System removed from **{guild.Name}** Deleted **{numberOfTickets}** tickets."
                };
                await context.CreateResponseAsync(embed);

            } else
            {
                DiscordGuild guild = context.Guild;
                int numberOfTickets = await DeleteTicketSystemFromGuildAsync(guild);
                embed = new DiscordEmbedBuilder
                {
                    Title = "Ticket System",
                    Color = DiscordColor.Green,
                    Description = $"Ticket System removed from **{guild.Name}** Deleted **{numberOfTickets}** tickets."
                };
                await context.CreateResponseAsync(embed);
            }
        }


        [SlashCommand("close", "Close the ticket")]
        [SlashRequireGuild]
        [RequirePermissions(Permissions.ManageChannels)]
        public async Task CloseTicket(InteractionContext context,
                                       [Option("Channel", "Mention ticket to close")] DiscordChannel channel = null)
        {
            DiscordEmbedBuilder embed;
            if (channel != null)
            {
                await DeleteTicketFromDatabaseAsync(channel.Id);

                embed = new DiscordEmbedBuilder
                {
                    Title = "Ticket System",
                    Description = "This channel will be closed in 5 seconds.",
                    Color = DiscordColor.SapGreen
                };
                await context.CreateResponseAsync(embed);

                await Task.Delay(TimeSpan.FromSeconds(5));

                await channel.DeleteAsync();
            } else
            {
                channel = context.Channel;
                await DeleteTicketFromDatabaseAsync(channel.Id);

                embed = new DiscordEmbedBuilder
                {
                    Title = "Ticket System",
                    Description = "This channel will be closed in 5 seconds.",
                    Color = DiscordColor.SapGreen
                };
                await context.CreateResponseAsync(embed);

                await Task.Delay(TimeSpan.FromSeconds(5));

                await channel.DeleteAsync();
            }

        }

        //
        // Functions
        //

        public async Task<int> DeleteTicketSystemFromGuildAsync(DiscordGuild guild)
        {
            var dbManager = Program.dbManager;
            int deletedTicketsCount = 0;

            List<ulong> channelIds = await GetTicketChannelsForGuildAsync(guild);

            string deleteSetupQuery = @"DELETE FROM ticketsystem WHERE GuildId = @GuildId";
            string deleteTicketsQuery = @"DELETE FROM ticket WHERE GuildID = @GuildId";

            try
            {
                await dbManager.OpenConnectionAsync();
                using (MySqlCommand command = dbManager.CreateCommand("SELECT ChannelId, MessageId FROM ticketsystem WHERE GuildId = @GuildId"))
                {
                    ulong channelId;
                    ulong messageId;

                    command.Parameters.AddWithValue("@GuildId", guild.Id);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            channelId = Convert.ToUInt64(reader["ChannelId"]);
                            messageId = Convert.ToUInt64(reader["MessageId"]);

                            try
                            {
                                DiscordChannel channel = guild.Channels[channelId];
                                if (channel != null)
                                {
                                    DiscordMessage message = await channel.GetMessageAsync(messageId);
                                    if (message != null)
                                    {
                                        await message.DeleteAsync();
                                    }
                                }
                            } catch (Exception ex)
                            {
                                ConsoleColors.WriteLineWithColors($"[ ^4Ticket ^0] [ ^1Error ^0] Error while delete ticket message ! {ex.Message}");
                            }
                        } else
                        {
                            ConsoleColors.WriteLineWithColors($"[ ^4Ticket ^0] [ ^1Error ^0] Cannot found the ticket Channel / Message while deleting !");
                        }
                    }
                }


                // Delete ticket system setup
                using (MySqlCommand setupCommand = dbManager.CreateCommand(deleteSetupQuery))
                {
                    setupCommand.Parameters.AddWithValue("@GuildId", guild.Id);
                    await setupCommand.ExecuteNonQueryAsync();
                }

                // Delete tickets associated with the guild and retrieve the count of deleted rows
                using (MySqlCommand ticketsCommand = dbManager.CreateCommand(deleteTicketsQuery))
                {
                    ticketsCommand.Parameters.AddWithValue("@GuildId", guild.Id);
                    deletedTicketsCount = await ticketsCommand.ExecuteNonQueryAsync();
                }

                // Log success message if needed
                ConsoleColors.WriteLineWithColors("[ ^4Ticket ^0] [ ^2Success ^0] Ticket system removed from the guild.");

                // Delete the fetched channels
                foreach (var channelId in channelIds)
                {
                    var channel = guild.GetChannel(channelId);
                    if (channel != null)
                    {
                        await channel.DeleteAsync();
                    }
                }

                return deletedTicketsCount;
            } catch (Exception ex)
            {
                ConsoleColors.WriteLineWithColors($"[ ^4Ticket ^0] [ ^1Error ^0] Error removing ticket system from the guild: {ex.Message}");
                await Program._logger.LogErrorAsync(ex);

                return deletedTicketsCount;
            } finally
            {
                await dbManager.CloseConnectionAsync();
            }

        }

        private async Task<List<ulong>> GetTicketChannelsForGuildAsync(DiscordGuild guild)
        {
            var dbManager = Program.dbManager;
            List<ulong> channelIds = new List<ulong>();

            string selectQuery = @"
    SELECT ChannelID 
    FROM ticket 
    WHERE GuildID = @GuildId";

            try
            {
                await dbManager.OpenConnectionAsync();

                using (MySqlCommand command = dbManager.CreateCommand(selectQuery))
                {
                    command.Parameters.AddWithValue("@GuildId", guild.Id);

                    // Execute query and fetch channel IDs
                    using (MySqlDataReader reader = await command.ExecuteReaderAsync(System.Data.CommandBehavior.Default))
                    {
                        while (await reader.ReadAsync())
                        {
                            ulong channelId = reader.GetUInt64("ChannelID");
                            channelIds.Add(channelId);
                        }
                    }
                }

                return channelIds;
            } catch (Exception ex)
            {
                ConsoleColors.WriteLineWithColors($"[ ^4Ticket ^0] [ ^1Error ^0] Error fetching ticket channels: {ex.Message}");
                await Program._logger.LogErrorAsync(ex);
                return channelIds;
            } finally
            {
                await dbManager.CloseConnectionAsync();
            }
        }



        public async Task DeleteTicketFromDatabaseAsync(ulong channelId)
        {
            var dbManager = Program.dbManager;

            string deleteQuery = @"
        DELETE FROM Ticket 
        WHERE ChannelID = @ChannelID";

            try
            {
                await dbManager.OpenConnectionAsync();
                using (MySqlCommand command = dbManager.CreateCommand(deleteQuery))
                {
                    command.Parameters.AddWithValue("@ChannelID", channelId);

                    await command.ExecuteNonQueryAsync();
                }
            } catch (Exception ex)
            {
                ConsoleColors.WriteLineWithColors($"[ ^4Ticket ^0] [ ^1Error ^0] Error deleting ticket from the database: {ex.Message}");
                await Program._logger.LogErrorAsync(ex);
            } finally
            {
                await dbManager.CloseConnectionAsync();
            }
        }


        public async Task<DiscordChannel> CreateChannelAsync(DiscordGuild guild, string type, DiscordUser user)
        {
            bool isSetup = await IsTicketSystemSetupAsync(guild.Id);

            var data = await GetTicketSystemSetupAsync(guild.Id);

            if (isSetup && data.ContainsKey("CategoryId"))
            {
                ulong categoryId = Convert.ToUInt64(data["CategoryId"]);
                var categoryPair = guild.Channels.FirstOrDefault(pair => pair.Value.Id == categoryId);

                if (categoryPair.Value != null)
                {
                    var category = categoryPair.Value as DiscordChannel;

                    try
                    {
                        DiscordChannel channel = await guild.CreateChannelAsync($"ticket-{type}-{user.Username}", ChannelType.Text, category);
                        await channel.AddOverwriteAsync(guild.EveryoneRole, Permissions.None, Permissions.All);
                        DiscordMember member = await guild.GetMemberAsync(user.Id);
                        await channel.AddOverwriteAsync(member, Permissions.SendMessages, Permissions.None);


                        return channel;
                    } catch (Exception ex)
                    {
                        ConsoleColors.WriteLineWithColors($"[ ^4Ticket ^0] [ ^1Error ^0] {ex}");
                    }
                } else
                {
                    ConsoleColors.WriteLineWithColors("[ ^4Ticket ^0] [ ^1Error ^0] Category not found.");
                    return null;
                }
            } else
            {
                ConsoleColors.WriteLineWithColors("[ ^4Ticket ^0] [ ^1Error ^0] Ticket system is not properly set up.");
                return null;
            }
            return null;
        }



        public async Task<bool> IsTicketSystemSetupAsync(ulong guildId)
        {
            var dbm = Program.dbManager;
            bool isSetup = false;

            try
            {
                await dbm.OpenConnectionAsync();

                MySqlCommand command = dbm.CreateCommand(
                    "SELECT EXISTS (SELECT 1 FROM ticketsystem WHERE GuildId = @GuildId)"
                );

                command.Parameters.AddWithValue("@GuildId", guildId);

                object result = await command.ExecuteScalarAsync();

                if (result != null && result != DBNull.Value)
                {
                    isSetup = Convert.ToBoolean(result);
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await Program._logger.LogErrorAsync(ex);
            } finally
            {
                dbm.CloseConnection();
            }

            return isSetup;
        }



        private async Task<Dictionary<string, object>> GetTicketSystemSetupAsync(ulong guildId)
        {
            var dbm = Program.dbManager;
            Dictionary<string, object> rowData = new Dictionary<string, object>();

            try
            {
                await dbm.OpenConnectionAsync();

                MySqlCommand command = dbm.CreateCommand("SELECT * FROM ticketsystem WHERE GuildId = @GuildId");
                command.Parameters.AddWithValue("@GuildId", guildId);

                using (MySqlDataReader reader = await command.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow))
                {
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                rowData[reader.GetName(i)] = reader.GetValue(i);
                            }
                        }
                    }
                }
            } catch (Exception ex)
            {
                ConsoleColors.WriteLineWithColors($"[ ^4Ticket ^0] [ ^1Error ^0] {ex}");
            } finally
            {
                await dbm.CloseConnectionAsync();
            }

            return rowData;
        }

        public async Task SaveTicketToDatabaseAsync(nTicket ticket)
        {
            var dbManager = Program.dbManager;

            string insertQuery = @"
        INSERT INTO Ticket (GuildID, ChannelID, DiscordUserID, Status, Type)
        VALUES (@GuildID, @ChannelID, @DiscordUserID, @Status, @Type)";

            try
            {
                await dbManager.OpenConnectionAsync();
                using (MySqlCommand command = dbManager.CreateCommand(insertQuery))
                {
                    command.Parameters.AddWithValue("@GuildID", ticket.GuildID);
                    command.Parameters.AddWithValue("@ChannelID", ticket.ChannelID);
                    command.Parameters.AddWithValue("@DiscordUserID", ticket.DiscordUserID);
                    command.Parameters.AddWithValue("@Status", "Open");
                    command.Parameters.AddWithValue("@Type", ticket.Type.ToString());

                    await command.ExecuteNonQueryAsync();
                }
            } catch (Exception ex)
            {
                ConsoleColors.WriteLineWithColors($"[ ^4Ticket ^0] [ ^1Error ^0] {ex}");
                await Program._logger.LogErrorAsync(ex);
            } finally
            {
                await dbManager.CloseConnectionAsync();
            }
        }



    }
}
