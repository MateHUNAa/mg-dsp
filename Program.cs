using DiscordBotTemplateNet7.Commands;
using DiscordBotTemplateNet7.Config;
using DiscordBotTemplateNet7.EventSystem;
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
using mh_Auth;
using mh_Auth.Interface;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using System.Data;
using System.Linq.Expressions;
using System.Net;

namespace DiscordBotTemplateNet7
{
    public sealed class Program
    {
        private static Timer leaderboardTimer;
        public static DiscordClient Client { get; private set; }
        public static CommandsNextExtension Commands { get; private set; }
        public static DatabaseManager dbManager { get; private set; }
        public static WebServerHandler webHandler { get; private set; }
        public static EventPublisher publisher { get; private set; }

        private static readonly string connectionString = "Server=localhost;Database=hwid_lic;Uid=matehun;Password=admin123";
        public static Logger _logger { get; private set; }

        private utility u = new utility();

        private static string token;
        private static string prefix;
        private static string version;
        private static string license;


        public static readonly bool userAgenLock = false;

        public static Auth a { get; private set; }
        public static Auth.Utility au { get; private set; }

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

                case "previus-page":
                    await e.Interaction.DeferAsync(true);
                    await e.Interaction.DeleteOriginalResponseAsync();
                    break;

                case "next-page":
                    await e.Interaction.DeferAsync(true);
                    await e.Interaction.DeleteOriginalResponseAsync();
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

        private async Task CheckExpiredVIPStatus(DiscordClient client)
        {
#if DEBUG
            ConsoleColors.WriteLineWithColors("[ ^4ExpiredVIP ^0]: Checking for expired VIP's");
#endif

            Rimedm rime = new Rimedm();
            try
            {
                await dbManager.OpenConnectionAsync();

                MySqlCommand cmd = dbManager.CreateCommand("SELECT discordId, roleId, experiation_date FROM `mate_vipsystem`");
                ulong roleId = 0;
                ulong discordId = 0;
                DateTime expirationDate = DateTime.MinValue;
                using (MySqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    while (await reader.ReadAsync())
                    {
                        discordId = reader.GetUInt64("discordId");
                        roleId = reader.GetUInt64("roleId");
                        expirationDate = reader.GetDateTime("experiation_date");
                    }

                }

                if (roleId != 0 && expirationDate != DateTime.MinValue)
                {
                    ulong guildId = 0;
                    try
                    {
                        await dbManager.OpenConnectionAsync();
                        MySqlCommand cmd2 = dbManager.CreateCommand("SELECT guildId FROM `vip_roles` WHERE roleId = @roleId");
                        cmd2.Parameters.AddWithValue("@roleId", roleId);

                        using (MySqlDataReader reader2 = await cmd2.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                        {
                            while (await reader2.ReadAsync())
                            {
                                guildId = reader2.GetUInt64("guildId");
                            }
                        }
                    } catch (Exception ex)
                    {
                        await _logger.LogErrorAsync(ex);
                    } finally
                    {
                        await dbManager.CloseConnectionAsync();
                    }


                    if (DateTime.UtcNow > expirationDate)
                    {
                        // Expired VIP logic
                        if (guildId != 0)
                        {
                            DiscordGuild guild = await Client.GetGuildAsync(guildId);
                            DiscordUser user = await guild.GetMemberAsync(discordId);
                            if (user != null)
                            {
                                DiscordRole vipRole = await rime.GetUserVIPRole(user, guild);
                                if (vipRole != null)
                                {
                                    try
                                    {
                                        await dbManager.OpenConnectionAsync();
                                        DiscordMember member = await guild.GetMemberAsync(user.Id);
                                        await member.RevokeRoleAsync(vipRole);

                                        MySqlCommand deleteVIP = dbManager.CreateCommand("DELETE FROM `mate_vipsystem` WHERE discordId = @discordId");
                                        deleteVIP.Parameters.AddWithValue("@discordId", discordId);
                                        await deleteVIP.ExecuteNonQueryAsync();

                                        DiscordChannel channel = guild.Channels.Values.FirstOrDefault(ch => ch.Name.Equals("expired-vips", StringComparison.OrdinalIgnoreCase));

                                        if (channel == null)
                                        {
                                            channel = await guild.CreateChannelAsync("expired-vips", ChannelType.Text);
                                        }

                                        DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                                        {
                                            Title = "Rime VIP",
                                            Description = $"{user.Mention} VIP status was expired !",
                                            Color = DiscordColor.Aquamarine,
                                            Timestamp = DateTime.UtcNow
                                        };

                                        await channel.SendMessageAsync(embed: embed);
                                    } catch (Exception ex)
                                    {
                                        await _logger.LogErrorAsync(ex);
                                    } finally
                                    {
                                        await dbManager.CloseConnectionAsync();
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex);
            } finally
            {
                await dbManager.CloseConnectionAsync();
            }
        }



        private async Task StartLeaderboardUpdateAsync(DiscordClient c)
        {
            var dbm = Program.dbManager;

            try
            {
                await dbm.OpenConnectionAsync();

                var command = dbm.CreateCommand("SELECT * FROM `leaderboard`");
                using var reader = await command.ExecuteReaderAsync();

                var guilds = new List<(DiscordGuild Guild, ulong MessageId, ulong ChannelId)>();

                while (await reader.ReadAsync())
                {
                    var guildId = reader.GetInt64("guildid");
                    var channelId = reader.GetInt64("channelId");
                    var messageId = reader.GetInt64("messageId");

                    DiscordGuild guild = await Client.GetGuildAsync((ulong)guildId);
                    guilds.Add((guild, (ulong)messageId, (ulong)channelId));

                }

                await reader.CloseAsync();

                foreach (var (guild, messageId, channelId) in guilds)
                {
                    await RefreshLeaderboardAsync(c, guild, messageId, channelId);
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            } finally
            {
                await dbm.CloseConnectionAsync();
            }
        }

        private async Task RefreshLeaderboardAsync(DiscordClient client, DiscordGuild guild, ulong messageId, ulong channelId)
        {
            var dbm = Program.dbManager;

            try
            {
                string query = "SELECT * FROM `mate_kd` ORDER BY kills DESC LIMIT 10";
                using var reader = await dbm.ExecuteReaderAsync(query);

                var embedBuilder = new DiscordEmbedBuilder()
                    .WithTitle("Top 10 Users! Order by kills")
                    .WithColor(DiscordColor.Gold)
                    .WithFooter($"Last update - {DateTime.UtcNow.ToString("HH:mm:ss")} UTC")
                    .WithTimestamp(DateTime.UtcNow);


                while (await reader.ReadAsync())
                {
                    int kills = reader.GetInt32("kills");
                    int deaths = reader.GetInt32("deaths");
                    int headshot = reader.GetInt32("headshot");
                    string username = reader.GetString("player_name");

                    embedBuilder.AddField($"User: {username}", $"Kills: {kills}, Deaths: {deaths}, Headshots: {headshot}");

                }

                var channel = guild.GetChannel(channelId) as DiscordChannel;
                if (channel != null)
                {
                    var messages = await channel.GetMessagesAsync();
                    foreach (var message in messages)
                    {
                        await channel.DeleteMessageAsync(message);
                    }
                    await channel.SendMessageAsync(embed: embedBuilder.Build());
                } else
                {
                    Console.Write($"Channel with ID {channelId} not found in guild {guild.Name}.");
                }
            } catch (Exception ex)
            {
                await Program._logger.LogErrorAsync(ex);
            }
        }




        public async Task InitializeAsync()
        {

            await LoadBotConfigurationsAsync();
            ConfigureDiscordClient();
            RegisterCommandsAndInteractions();
            InitializeStuff();
            //InitializeWebServer();


            // License Key Check
            Thread thread = new Thread(new ThreadStart(checkingConnection)) { IsBackground = true };
            //thread.Start();

            Thread tLeaderboard = new Thread(new ThreadStart(delayedTasks)) { IsBackground = true };
            tLeaderboard.Start();
        }



        private async void delayedTasks()
        {
            while (true)
            {
                await StartLeaderboardUpdateAsync(Client);
                await CheckExpiredVIPStatus(Client);
                await Task.Delay(30000);
            }
        }

        private async Task LoadBotConfigurationsAsync()
        {
            BotConfig _config = new BotConfig();
            await _config.ReadJSONAsync();
            token = _config.Token;
            prefix = _config.Prefix;
            version = _config.Version;
            license = _config.license;

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
            slashConfig.RegisterCommands<Ticket>();
            slashConfig.RegisterCommands<FivemProfile>();
            slashConfig.RegisterCommands<Lacskak>(1221813069634732042);
            slashConfig.RegisterCommands<Rimedm>(741304300319539223);

        }

        private void InitializeStuff()
        {
            _logger = new Logger(Client);
            dbManager = new DatabaseManager(connectionString);
            publisher = new EventPublisher();

            MySqlConnection conn = new MySqlConnection(connectionString);
            a = new Auth(conn);
            au = new Auth.Utility();


        }


        private static async void checkingConnection()
        {
            while (true)
            {

                string HWID = Auth.GetHWID();
                string IP = await au.GetPublicIpAddress();
                IUserInputHandler handler = new ConsoleInputHandler();

                if (await a.Authenticate(license, HWID, IP, handler))
                {
                    ConsoleColors.WriteLineWithColors("[ ^4License^0 ] [ ^3INFO ^0] Key is valid !");
                } else
                {
                    ConsoleColors.WriteLineWithColors("[ ^4License^0 ] [ ^1ERROR ^0] Key is expired or not valid!");
                    Environment.Exit(0000256600 + 176);
                }

                await Task.Delay(15000);
            }
        }

        private void InitializeWebServer()
        {
            string url = "http://localhost:8080/";
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(url);

            ConsoleColors.WriteLineWithColors($"[ ^4Program ^0] [ ^2WebListener ^0] Listening on ^2{url}");

            listener.Start();

            webHandler = new WebServerHandler();

            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    HttpListenerContext context = listener.GetContext();
                    webHandler.ProcessRequest(context);
                }
            });

            thread.Start();

        }

    }
}