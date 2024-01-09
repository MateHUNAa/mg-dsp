using DiscordBotTemplateNet7.Commands;
using DiscordBotTemplateNet7.Config;
using DiscordBotTemplateNet7.Repositories;
using DiscordBotTemplateNet7.Slash_Commands;
using DiscordBotTemplateNet7.Utility;
using DiscordBotTemplateNet7.Valami;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Security;
using System;
using System.Net.WebSockets;

namespace DiscordBotTemplateNet7
{
    public sealed class Program
    {
        private static Timer leaderboardTimer;
        public static DiscordClient Client { get; private set; }
        public static CommandsNextExtension Commands { get; private set; }
        public static DatabaseManager dbManager { get; private set; }

        private static readonly string connectionString = "Server=localhost;Database=gm-ddp;Uid=root;";
        public static Logger _logger { get; private set; }

        private static string token;
        private static string prefix;
        private static string version;

        public static async Task Main(string[] args)
        {



            var programInstance = new Program();
            await programInstance.InitializeAsync();

            //
            // Events
            //

            Client.Ready += OnClientReady;
            Client.GuildCreated += onGuildCreated;
            Client.GuildDeleted += onGuildRemoved;
            Client.ComponentInteractionCreated += onInteractionExecuted;

            await Client.ConnectAsync();
            await Task.Delay(-1);



        }

        private static async Task onInteractionExecuted(DiscordClient s, ComponentInteractionCreateEventArgs e)
        {

            Ticket ticket = new Ticket();


            switch (e.Interaction.Data.CustomId)
            {
                case "tamogatas":
                    try
                    {
                        await e.Interaction.DeferAsync(true);
                        await e.Interaction.DeleteOriginalResponseAsync();
                        DiscordChannel channel = await ticket.CreateChannelAsync(e.Guild, e.Interaction.Data.CustomId, e.User);
                        var embed = new DiscordEmbedBuilder
                        {
                            Title = "Ticket System 🎫",
                            Description = "Welcome to the ticket system! Please wait patiently, and our support team will reach out to assist you. ⏳✨",
                            Color = DiscordColor.Green
                        };
                        if (channel != null)
                        {
                            await channel.SendMessageAsync(embed: embed);

                            nTicket Ticket = new nTicket();

                            Ticket.Type = Ticket.GetTypeFromCustomId(e.Interaction.Data.CustomId);
                            Ticket.GuildID = e.Guild.Id;
                            Ticket.ChannelID = channel.Id;
                            Ticket.DiscordUserID = e.User.Id;


                            UserRepository userRepo = new UserRepository();
                            bool isUserExist = await userRepo.CheckUserExistenceAsync(e.User.Id);

                            if (isUserExist)
                            {
                                await ticket.SaveTicketToDatabaseAsync(Ticket);
                            } 
                            else
                            {
                                await userRepo.SaveDiscordUserAsync(e.User, e.Guild);
                                await ticket.SaveTicketToDatabaseAsync(Ticket);
                            }

                        } else
                        {
                            ConsoleColors.WriteLineWithColors("[[ ^4Ticket ^0] [ ^1Error ^0] Channel creation failed or returned null.");
                        }
                    } catch (Exception ex)
                    {
                        ConsoleColors.WriteLineWithColors($"[[ ^4Ticket ^0] [ ^1Error ^0] Exception while creating channel: {ex}");
                        await _logger.LogErrorAsync(ex);
                    }
                    break;

                case "bug":
                    try
                    {
                        await e.Interaction.DeferAsync(true);
                        await e.Interaction.DeleteOriginalResponseAsync();
                        DiscordChannel channel = await ticket.CreateChannelAsync(e.Guild, e.Interaction.Data.CustomId, e.User);
                        var embed = new DiscordEmbedBuilder
                        {
                            Title = "Ticket System 🎫",
                            Description = "Welcome to the ticket system! Please wait patiently, and our support team will reach out to assist you. ⏳✨",
                            Color = DiscordColor.Green
                        };
                        if (channel != null)
                        {
                            await channel.SendMessageAsync(embed: embed);

                            nTicket Ticket = new nTicket();

                            Ticket.Type = Ticket.GetTypeFromCustomId(e.Interaction.Data.CustomId);
                            Ticket.GuildID = e.Guild.Id;
                            Ticket.ChannelID = channel.Id;
                            Ticket.DiscordUserID = e.User.Id;


                            UserRepository userRepo = new UserRepository();
                            bool isUserExist = await userRepo.CheckUserExistenceAsync(e.User.Id);

                            if (isUserExist)
                            {
                                await ticket.SaveTicketToDatabaseAsync(Ticket);
                            } else
                            {
                                await userRepo.SaveDiscordUserAsync(e.User, e.Guild);
                                await ticket.SaveTicketToDatabaseAsync(Ticket);
                            }

                        } else
                        {
                            ConsoleColors.WriteLineWithColors("[[ ^4Ticket ^0] [ ^1Error ^0] Channel creation failed or returned null.");
                        }
                    } catch (Exception ex)
                    {
                        ConsoleColors.WriteLineWithColors($"[[ ^4Ticket ^0] [ ^1Error ^0] Exception while creating channel: {ex}");
                        await _logger.LogErrorAsync(ex);
                    }
                    break;

                case "frakciok":
                    try
                    {
                        await e.Interaction.DeferAsync(true);
                        await e.Interaction.DeleteOriginalResponseAsync();
                        DiscordChannel channel = await ticket.CreateChannelAsync(e.Guild, e.Interaction.Data.CustomId, e.User);
                        var embed = new DiscordEmbedBuilder
                        {
                            Title = "Ticket System 🎫",
                            Description = "Welcome to the ticket system! Please wait patiently, and our support team will reach out to assist you. ⏳✨",
                            Color = DiscordColor.Green
                        };
                        if (channel != null)
                        {
                            await channel.SendMessageAsync(embed: embed);

                            nTicket Ticket = new nTicket();

                            Ticket.Type = Ticket.GetTypeFromCustomId(e.Interaction.Data.CustomId);
                            Ticket.GuildID = e.Guild.Id;
                            Ticket.ChannelID = channel.Id;
                            Ticket.DiscordUserID = e.User.Id;


                            UserRepository userRepo = new UserRepository();
                            bool isUserExist = await userRepo.CheckUserExistenceAsync(e.User.Id);

                            if (isUserExist)
                            {
                                await ticket.SaveTicketToDatabaseAsync(Ticket);
                            } else
                            {
                                await userRepo.SaveDiscordUserAsync(e.User, e.Guild);
                                await ticket.SaveTicketToDatabaseAsync(Ticket);
                            }

                        } else
                        {
                            ConsoleColors.WriteLineWithColors("[[ ^4Ticket ^0] [ ^1Error ^0] Channel creation failed or returned null.");
                        }
                    } catch (Exception ex)
                    {
                        ConsoleColors.WriteLineWithColors($"[[ ^4Ticket ^0] [ ^1Error ^0] Exception while creating channel: {ex}");
                        await _logger.LogErrorAsync(ex);
                    }
                    break;

                case "admin_tgf":
                    try
                    {
                        await e.Interaction.DeferAsync(true);
                        await e.Interaction.DeleteOriginalResponseAsync();
                        DiscordChannel channel = await ticket.CreateChannelAsync(e.Guild, e.Interaction.Data.CustomId, e.User);
                        var embed = new DiscordEmbedBuilder
                        {
                            Title = "Ticket System 🎫",
                            Description = "Welcome to the ticket system! Please wait patiently, and our support team will reach out to assist you. ⏳✨",
                            Color = DiscordColor.Green
                        };
                        if (channel != null)
                        {
                            await channel.SendMessageAsync(embed: embed);

                            nTicket Ticket = new nTicket();

                            Ticket.Type = Ticket.GetTypeFromCustomId(e.Interaction.Data.CustomId);
                            Ticket.GuildID = e.Guild.Id;
                            Ticket.ChannelID = channel.Id;
                            Ticket.DiscordUserID = e.User.Id;


                            UserRepository userRepo = new UserRepository();
                            bool isUserExist = await userRepo.CheckUserExistenceAsync(e.User.Id);

                            if (isUserExist)
                            {
                                await ticket.SaveTicketToDatabaseAsync(Ticket);
                            } else
                            {
                                await userRepo.SaveDiscordUserAsync(e.User, e.Guild);
                                await ticket.SaveTicketToDatabaseAsync(Ticket);
                            }

                        } else
                        {
                            ConsoleColors.WriteLineWithColors("[[ ^4Ticket ^0] [ ^1Error ^0] Channel creation failed or returned null.");
                        }
                    } catch (Exception ex)
                    {
                        ConsoleColors.WriteLineWithColors($"[[ ^4Ticket ^0] [ ^1Error ^0] Exception while creating channel: {ex}");
                        await _logger.LogErrorAsync(ex);
                    }
                    break;

                default:
                    break;
            }


        }

        private static async Task onGuildRemoved(DiscordClient s, GuildDeleteEventArgs e)
        {
            DiscordGuild guild = e.Guild;
            dbManager.OpenConnection();

            MySqlCommand command = dbManager.CreateCommand(
                "DELETE FROM discordguild WHERE GuildId = @GuildId"
                );

            command.Parameters.AddWithValue("@GuildId", guild.Id);

            try
            {
                await command.ExecuteNonQueryAsync();
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await _logger.LogErrorAsync(ex);
            } finally
            {
                dbManager.CloseConnection();
                await _logger.LogLeaveAsync(guild);
            }

        }

        private static async Task onGuildCreated(DiscordClient s, GuildCreateEventArgs e)
        {

            DiscordGuild guild = e.Guild;

            dbManager.OpenConnection();

            MySqlCommand command = dbManager.CreateCommand(
                "INSERT INTO discordguild (GuildId, GuildName, GuildOwnerId, GuildMemberCount) " +
                "VALUES (@GuildId, @GuildName, @GuildOwnerId, @GuildMemberCount)"
                );

            command.Parameters.AddWithValue("@GuildId", guild.Id);
            command.Parameters.AddWithValue("@GuildName", guild.Name);
            command.Parameters.AddWithValue("@GuildOwnerId", guild.Owner.Id);
            command.Parameters.AddWithValue("@GuildMemberCount", guild.MemberCount);

            try
            {
                await command.ExecuteNonQueryAsync();
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await _logger.LogErrorAsync(ex);
            } finally
            {
                dbManager.CloseConnection();
                await _logger.LogJoinAsync(guild);
            }

        }

        private static async Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            ConsoleColors.WriteLineWithColors("[ ^4Program ^0] [ ^2Boot ^0]: Client Started");
            await _logger.LogBotStartedAsync(version);
        }

        private async Task RefreshLeaderboard(DiscordClient c)
        {
            ulong guildId = 741304300319539223;
            ulong channelId = 1187968495485452428;
            ulong errorId = 1187959648897208340;

            DiscordGuild guild = c.Guilds[guildId];

            string connectionString = "server=localhost;user=root;database=fivemserver";

            MySqlConnection connection = new MySqlConnection(connectionString);

            try
            {
                connection.Open();

                string query = "SELECT * FROM `mate_kd` ORDER BY kills DESC LIMIT 10";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                MySqlDataReader reader = cmd.ExecuteReader();

                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                    .WithTitle("Top 10 Users! Order by kills")
                    .WithColor(DiscordColor.Gold);

                while (reader.Read())
                {
                    int kills = reader.GetInt32("kills");
                    int deaths = reader.GetInt32("deaths");
                    int headshot = reader.GetInt32("headshot");
                    ulong discordId = reader.GetUInt64("discord_id");

                    DiscordMember member = await guild.GetMemberAsync(discordId, true);
                    string username = member?.Username ?? "User not Found";

                    embedBuilder.AddField($"User: {username}", $"Kills: {kills}, Deaths: {deaths}, Headshots: {headshot}");
                }

                reader.Close();

                DiscordChannel channel = await Client.GetChannelAsync(channelId);
                if (channel != null && channel is DiscordChannel textChannel)
                {
                    var messages = await textChannel.GetMessagesAsync();
                    foreach (var message in messages)
                    {
                        await textChannel.DeleteMessageAsync(message);
                    }
                    await textChannel.SendMessageAsync(embed: embedBuilder.Build());
                }


            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                DiscordChannel textChannel = await Client.GetChannelAsync(errorId);
                await textChannel.SendMessageAsync("[ERROR]: " + ex.Message);

            } finally
            {
                connection.Close();
            }
        }

        public async Task InitializeAsync()
        {

            await LoadBotConfigurationsAsync();
            ConfigureDiscordClient();
            RegisterCommandsAndInteractions();
            InitializeStuff();

        }

        private async Task LoadBotConfigurationsAsync()
        {
            BotConfig _config = new BotConfig();
            await _config.ReadJSONAsync();
            token = _config.Token;
            prefix = _config.Prefix;
            version = _config.Version;

        }


        private void ConfigureDiscordClient()
        {

            var config = new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents = DiscordIntents.All
            };
            Client = new DiscordClient(config);

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(3)
            });
        }

        private void RegisterCommandsAndInteractions()
        {

            Commands = Client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] { prefix },
                EnableDms = true,
                EnableMentionPrefix = true,
                EnableDefaultHelp = false
            });

            Commands.RegisterCommands<Basic>();

            var slashConfig = Client.UseSlashCommands();
            slashConfig.RegisterCommands<Core>();
            slashConfig.RegisterCommands<Ticket>(741304300319539223);
        }

        private void InitializeStuff()
        {
            _logger = new Logger(Client);
            dbManager = new DatabaseManager(connectionString);
        }


    }
}