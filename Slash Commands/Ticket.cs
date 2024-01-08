using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotTemplateNet7.Slash_Commands
{
    public class Ticket : ApplicationCommandModule
    {

        [SlashCommand("ticket-setup", "Setup a Ticket Channel")]
        public async Task SetupTicketSystem(InteractionContext context,
                                            [Option("Channel", "The Channel to set up the ticket system in")] DiscordChannel channel)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Ticket System",
                Description = "Welcome to the Ticket section. Please select how our support team can assist you.",
                Color = DiscordColor.Green
            };

            var row = new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Primary, "tamogatas", "Tamogatas"),
                    new DiscordButtonComponent(ButtonStyle.Primary, "bug", "BUG"),
                    new DiscordButtonComponent(ButtonStyle.Primary, "frakciok", "Frakciok"),
                    new DiscordButtonComponent(ButtonStyle.Primary, "admin_tgf", "Admin TGF")
                };

            var builder = new DiscordMessageBuilder().WithEmbed(embed).AddComponents(row);
            await channel.SendMessageAsync(builder);


            context.Client.ComponentInteractionCreated += onInteractionCreated;

        }

        private Task onInteractionCreated(DiscordClient s, DSharpPlus.EventArgs.ComponentInteractionCreateEventArgs e)
        {

            switch (e.Interaction.Data.CustomId)
            {
                case "tamogatas":

                    break;

                default: break;
            }

            return Task.CompletedTask;
        }
    }
}
