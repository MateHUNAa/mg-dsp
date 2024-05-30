using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;

namespace DiscordBotTemplateNet7.Slash_Commands
{
    public class Core : ApplicationCommandModule
    {

        private readonly string connectionString = "";

        private readonly string connectionFivemServer = "";

        [SlashCommand("registerResource", "Register a resource")]
        public async Task RegisterResource(InteractionContext ctx,
                                       [Option("Resname", "The name of the resource")] string resname,
                                       [Option("Customer", "Customer name")] string customer,
                                       [Option("customerDiscordId", "Customer's Discord ID")] string customerDiscordId,
                                       [Option("_description", "Description of the resource")] string description)
        {



            MySqlConnection connection = new MySqlConnection(connectionString);

            var util = new Utility.utility();

            try
            {
                connection.Open();

                string insertQuery = "INSERT INTO module_keys (resourceKey, resName, customer, customerDiscordId, _description) VALUES (@resourceKey, @resName, @customer, @customerDiscordId, @_description)";
                using MySqlCommand command = new MySqlCommand(insertQuery, connection);
                string key = "matehun-" + util.GenerateRandomBase64Key(55);

                command.Parameters.AddWithValue("@resourceKey", key);
                command.Parameters.AddWithValue("@resName", resname);
                command.Parameters.AddWithValue("@customer", customer);
                command.Parameters.AddWithValue("@customerDiscordId", customerDiscordId);
                command.Parameters.AddWithValue("@_description", description);

                await command.ExecuteNonQueryAsync();
                DiscordInteractionResponseBuilder response = new DiscordInteractionResponseBuilder();
                response.WithContent("Resource registered successfully !\n||" + key + "||");
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);

            } catch (Exception ex)
            {
                Console.WriteLine(ex);
            } finally
            {
                connection.Close();
            }

        }

    }
}
