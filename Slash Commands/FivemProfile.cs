using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotTemplateNet7.Slash_Commands
{
    public class FivemProfile : ApplicationCommandModule
    {
        [SlashCommand("profPic", "Get ProfPic")]
        public async Task ProfPic(InteractionContext context)
        {
            DiscordUser user = context.User;
            DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder();
            builder.WithContent($"`{user.AvatarUrl}`");
            await context.CreateResponseAsync(builder);
        }
    }
}
