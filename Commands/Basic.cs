// Ensure to include necessary namespaces
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MySql.Data.MySqlClient;
using System.Text;

namespace DiscordBotTemplateNet7.Commands
{
    public class Basic : BaseCommandModule
    {
        [Command("getTop10")]
        public async Task TestCommand(CommandContext ctx)
        {
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

                    DiscordMember member = await ctx.Guild.GetMemberAsync(discordId, true);
                    string username = member?.Username ?? "User not Found";

                    embedBuilder.AddField($"User: {username}", $"Kills: {kills}, Deaths: {deaths}, Headshots: {headshot}");
                }

                reader.Close();

                await ctx.Channel.SendMessageAsync(embed: embedBuilder.Build());


            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                await ctx.RespondAsync($"Error: {ex.Message}");
            } finally
            {
                connection.Close();
            }
        }
    }
}
