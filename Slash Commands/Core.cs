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

        //end

        [SlashCommand("setvip", "Set The Player Vip Level")]
        [RequireRoles(RoleCheckMode.MatchIds, 930120114592948315)]
        public async Task SetVIP(InteractionContext context,
                                 [Option("user", "The user to set VIP status for")] DiscordUser user,
                                 [Option("level", "The VIP level")] long level,
                                 [Option("duration", "Experiation date for VIP Status")] string durationStr)
        {
            DiscordInteractionResponseBuilder response = new DiscordInteractionResponseBuilder();

            DiscordGuild guild = await context.Client.GetGuildAsync(context.Guild.Id);
            DiscordMember member = await guild.GetMemberAsync(user.Id);

            DiscordRole helperRole = guild.GetRole(1188551249910562817);
            DiscordRole vipRole = guild.GetRole(1188551297209749536);
            DiscordRole superVipRole = guild.GetRole(1188551329375846551);

            bool hasRole = member.Roles.Contains(vipRole) || member.Roles.Contains(superVipRole);

            if (hasRole)
            {
                response.WithContent("Player already has a VIP[Level] Role! Use inseted /updatevip");
                await context.CreateResponseAsync(response);
                return;
            }

            var util = new Utility.utility();
            DateTimeOffset experiationDate;
            if (util.TryParseDuration(durationStr, out TimeSpan duration))
            {
                experiationDate = DateTimeOffset.UtcNow.Add(duration);

            } else
            {
                throw new ArgumentException("Invalid duration format. Please provide the duration in a valid format (e.g., '5d').");
            }

            MySqlConnection connection = new MySqlConnection(connectionFivemServer);
            try
            {
                connection.Open();
                string insertQuery = "INSERT INTO mate_vipsystem (discordId, level, experiation_date, create_date) VALUES (@discordId, @level, @experiation_date, @create_date)";
                MySqlCommand command = new MySqlCommand(insertQuery, connection);

                command.Parameters.AddWithValue("@discordId", user.Id);
                command.Parameters.AddWithValue("@level", level);
                command.Parameters.AddWithValue("@experiation_date", experiationDate);
                command.Parameters.AddWithValue("@create_date", DateTime.Now);

                await command.ExecuteNonQueryAsync();





                await member.GrantRoleAsync(helperRole);


                if (level == 1)
                {
                    await member.GrantRoleAsync(vipRole);
                } else if (level == 2)
                {
                    await member.GrantRoleAsync(superVipRole);
                }


                response.WithContent($"Succsessfully added ${user.Username} to VIPs With level: ${level}");
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);


            } catch (Exception Ex)
            {
                Console.WriteLine(Ex);
            } finally
            {
                connection.Close();
            }

        }

        [SlashCommand("updatevip", "Update Player VIP Level ( VIP / SuperVip )")]
        [RequireRoles(RoleCheckMode.MatchIds, 930120114592948315)]
        public async Task UpdateVIP(InteractionContext context,
                                    [Option("user", "User to update vip.level")] DiscordUser user,
                                    [Option("level", "The New vip.level")] long level)
        {

            DiscordInteractionResponseBuilder response = new DiscordInteractionResponseBuilder();

            DiscordGuild guild = await context.Client.GetGuildAsync(context.Guild.Id);
            DiscordMember member = await guild.GetMemberAsync(user.Id);

            DiscordRole helperRole = guild.GetRole(1188551249910562817);
            DiscordRole vipRole = guild.GetRole(1188551297209749536);
            DiscordRole superVipRole = guild.GetRole(1188551329375846551);

            bool hasRole = member.Roles.Contains(vipRole) || member.Roles.Contains(superVipRole);

            if (!hasRole)
            {
                response.WithContent("The player dosent have VIP ! Use insted /setvip");
                await context.CreateResponseAsync(response);
                return;
            }
            MySqlConnection connection = new MySqlConnection(connectionFivemServer);

            if (member.Roles.Contains(vipRole))
            {   // Can Update only to SuperVip
                if (level == 1)
                {
                    response.WithContent("You cannot set the new vip.level into the old vip.level");
                    await context.CreateResponseAsync(response);
                    return;
                }


                try
                {
                    connection.Open();

                    string query = "UPDATE mate_vipsystem SET level = @newLevel WHERE discordId = @discordId";
                    MySqlCommand command = new MySqlCommand(query, connection);

                    command.Parameters.AddWithValue("@newLevel", level);
                    command.Parameters.AddWithValue("@discordId", user.Id);

                    await command.ExecuteNonQueryAsync();

                    await member.RevokeRoleAsync(vipRole);
                    await member.GrantRoleAsync(superVipRole);

                } catch (Exception ex)
                {
                    Console.WriteLine(ex);
                } finally { connection.Close(); }

            } else if (member.Roles.Contains(superVipRole))
            {   // Can Update only to Vip
                if (level == 2)
                {
                    response.WithContent("You cannot set the new vip.level into the old vip.level");
                    await context.CreateResponseAsync(response);
                    return;
                }

                try
                {
                    connection.Open();

                    string query = "UPDATE mate_vipsystem SET level = @newLevel WHERE discordId = @discordId";
                    MySqlCommand command = new MySqlCommand(query, connection);

                    command.Parameters.AddWithValue("@newLevel", level);
                    command.Parameters.AddWithValue("@discordId", user.Id);

                    await command.ExecuteNonQueryAsync();
                    await member.RevokeRoleAsync(superVipRole);
                    await member.GrantRoleAsync(vipRole);
                } catch (Exception ex)
                {
                    Console.WriteLine(ex);
                } finally { connection.Close(); }
            }

            response.WithContent("Succsessfully changed the vip.level into " + level);
            await context.CreateResponseAsync(response);


        }
        [SlashCommand("delvip", "Remove Player VIP status")]
        [RequireRoles(RoleCheckMode.MatchIds, 930120114592948315)]
        public async Task RemoveVIP(InteractionContext context,
                                    [Option("user", "The user to revoke VIP status")] DiscordUser user)
        {

            DiscordInteractionResponseBuilder response = new DiscordInteractionResponseBuilder();

            DiscordGuild guild = await context.Client.GetGuildAsync(context.Guild.Id);
            DiscordMember member = await guild.GetMemberAsync(user.Id);

            DiscordRole helperRole = guild.GetRole(1188551249910562817);
            DiscordRole vipRole = guild.GetRole(1188551297209749536);
            DiscordRole superVipRole = guild.GetRole(1188551329375846551);

            bool hasRole = member.Roles.Contains(vipRole) || member.Roles.Contains(superVipRole);

            if (!hasRole)
            {
                response.WithContent("This Player dose not have VIP status.");
                await context.CreateResponseAsync(response);
                return;
            }

            MySqlConnection connection = new MySqlConnection(connectionFivemServer);

            try
            {
                connection.Open();

                string query = "DELETE FROM mate_vipsystem WHERE discordId = @discordId";
                MySqlCommand command = new MySqlCommand(query, connection);

                command.Parameters.AddWithValue("@discordId", user.Id);

                await command.ExecuteNonQueryAsync();

                if (member.Roles.Contains(vipRole))
                {
                    await member.RevokeRoleAsync(vipRole);
                } else if (member.Roles.Contains(superVipRole))
                {
                    await member.RevokeRoleAsync(superVipRole);
                }

                response.WithContent("Succsessfully revoked VIP status from [" + user.Mention + "]");
                await context.CreateResponseAsync(response);

            } catch (Exception ex)
            {
                response.WithContent("Something went wrong! Please try again later !");
                await context.CreateResponseAsync(response);
                Console.WriteLine(ex);
            } finally
            {
                connection.Close();
            }


        }


    }
}
