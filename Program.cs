using DiscordBotTemplateNet7.Commands;
using DiscordBotTemplateNet7.Config;
using DiscordBotTemplateNet7.Slash_Commands;
using DiscordBotTemplateNet7.Utility;
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
using System.Net.WebSockets;

namespace DiscordBotTemplateNet7
{
    public sealed class Program
    {
        private static Timer leaderboardTimer;
        public static DiscordClient Client { get; private set; }
        public static CommandsNextExtension Commands { get; private set; }

        private static DatabaseManager dbManager;
        private static readonly string connectionString = "Server=localhost;Database=gm-ddp;Uid=root;";
        private static Logger _logger;

        private static string token;
        private static string prefix;

        public static async Task Main(string[] args)
        {



            var programInstance = new Program();
            await programInstance.InitializeAsync();

            //
            // Events
            //

            Client.Ready += OnClientReady;
            Client.GuildCreated += onGuildCreated;

            await Client.ConnectAsync();
            await Task.Delay(-1);



        }

        private static async Task onGuildCreated(DiscordClient s, GuildCreateEventArgs e)
        {

            DiscordGuild guild = e.Guild;

            dbManager.OpenConnection();

            MySqlCommand command = dbManager.CreateCommand(
                "INSERT INTO discordguild (guild_id, guild_name, guild_owner_id, guild_member_count) " +
                "VALUES (@GuildId, @GuildName, @GuildOwnerId, @GuildMemberCount)"
                );

            command.Parameters.AddWithValue("GuildId", guild.Id);
            command.Parameters.AddWithValue("GuildName", guild.Name);
            command.Parameters.AddWithValue("GuildOwnerId", guild.Owner.Id);
            command.Parameters.AddWithValue("GuildMemberCount", guild.MemberCount);

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
            await _logger.LogAsync("Bot Started");
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
            InitializeLogger();
            InitializeDBManager();

        }

        private async Task LoadBotConfigurationsAsync()
        {
            BotConfig _config = new BotConfig();
            await _config.ReadJSONAsync();
            token = _config.Token;
            prefix = _config.Prefix;

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
        }

        private void InitializeLogger()
        {
            _logger = new Logger(Client);
        }

        private void InitializeDBManager()
        {
            dbManager = new DatabaseManager(connectionString);
        }
    }
}