using DSharpPlus;
using DSharpPlus.Entities;
using System.Globalization;
using System.Security.Cryptography;

namespace DiscordBotTemplateNet7.Utility
{
    public class utility
    {
        public IEnumerable<string> SplitText(string text, int chunkSize)
        {
            for (int i = 0; i < text.Length; i += chunkSize)
            {
                if (i + chunkSize >= text.Length)
                {
                    yield return text.Substring(i);
                } else
                {
                    yield return text.Substring(i, chunkSize);
                }
            }
        }

        public bool TryParseDuration(string durationStr, out TimeSpan duration)
        {
            durationStr = durationStr.Replace(" ", "");

            if (TimeSpan.TryParseExact(durationStr, "d'd'", CultureInfo.InvariantCulture, out duration) ||
                TimeSpan.TryParseExact(durationStr, "h'h'", CultureInfo.InvariantCulture, out duration) ||
                TimeSpan.TryParseExact(durationStr, "m'm'", CultureInfo.InvariantCulture, out duration))
            {
                return true;
            }

            return false;
        }



        public string GenerateRandomBase64Key(int length)
        {

            byte[] randomBytes = new byte[length * 3 / 4];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            string base64String = Convert.ToBase64String(randomBytes);
            base64String = base64String.Replace("+", "").Replace("/", ""); // Remove '+' and '/' characters

            while (base64String.Length < length)
            {
                byte[] additionalRandomBytes = new byte[3];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(additionalRandomBytes);
                }
                string additionalBase64 = Convert.ToBase64String(additionalRandomBytes).Replace("+", "").Replace("/", "");
                base64String += additionalBase64;
            }

            return base64String.Substring(0, length); // Return only the requested length
        }
        public (bool hasDangerous, Permissions? dangerousPermission) HasDangerousPermissions(DiscordRole role)
        {
            Permissions[] dangerousPermissions =
            {
                Permissions.Administrator,
                Permissions.ManageChannels,
                Permissions.ManageGuild,
                Permissions.ManageRoles,
                Permissions.KickMembers,
                Permissions.BanMembers,
                Permissions.ViewAuditLog,
                Permissions.ModerateMembers,
                Permissions.ManageMessages,
            };
            foreach (Permissions permission in dangerousPermissions)
            {
                if (role.Permissions.HasFlag(permission))
                {

                    return (true, permission);
                }
            }
            return (false, Permissions.None);
        }
    }
}
