using DiscordBotTemplateNet7.Utility;
using DSharpPlus.Entities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotTemplateNet7.Repositories
{
    public class UserRepository
    {
        private readonly DatabaseManager dbManager;
        public UserRepository()
        {
            this.dbManager = Program.dbManager;
        }

        public async Task SaveDiscordUserAsync(DiscordUser user, DiscordGuild guild)
        {
            string insertQuery = @"
                INSERT INTO discorduser (UserId, Username, AvatarUrl)
                VALUES (@UserId, @Username, @AvatarUrl)
                ON DUPLICATE KEY UPDATE Username = @Username, AvatarUrl = @AvatarUrl";

            try
            {
                await dbManager.OpenConnectionAsync();
                using (MySqlCommand command = dbManager.CreateCommand(insertQuery))
                {
                    command.Parameters.AddWithValue("@UserId", user.Id);
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@AvatarUrl", user.AvatarUrl);

                    await command.ExecuteNonQueryAsync();
                    ConsoleColors.WriteLineWithColors("[ ^4User ^0] [ ^2Success ^0] User data saved successfully.");
                    await Program._logger.UserRepoLogAsync(user, guild);
                }
            } catch (Exception ex)
            {
                ConsoleColors.WriteLineWithColors($"[ ^4User ^0] [ ^1Error ^0] {ex}");
                await Program._logger.LogErrorAsync(ex);
            } finally
            {
                await dbManager.CloseConnectionAsync();
            }
        }


        public async Task<bool> CheckUserExistenceAsync(ulong userId)
        {
            bool userExists = false;

            try
            {
                string query = "SELECT COUNT(*) FROM discorduser WHERE UserId = @UserId";

                await dbManager.OpenConnectionAsync();
                using (MySqlCommand command = dbManager.CreateCommand(query))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    object result = await command.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        int count = Convert.ToInt32(result);
                        userExists = count > 0;
                    }
                }
            } catch (Exception ex)
            {
                // Handle exceptions, if necessary
                Console.WriteLine($"Error checking user existence in the database: {ex.Message}");
            } finally
            {
                await dbManager.CloseConnectionAsync();
            }

            return userExists;
        }

    }
}
