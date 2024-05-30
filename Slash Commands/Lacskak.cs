using DiscordBotTemplateNet7.Utility;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Org.BouncyCastle.Crypto.Engines;

namespace DiscordBotTemplateNet7.Slash_Commands
{
    public class Lacskak : ApplicationCommandModule
    {
        [SlashCommand("memberDrag", "Drag Peoples b2w 2 channels")]
        [SlashCooldown(1, 10, SlashCooldownBucketType.Global)]
        public async Task DragPeoples(InteractionContext context, [Option("user", "The User")] DiscordUser user,
                                                             [Option("channel1", "Channel 1")] DiscordChannel channel_1,
                                                             [Option("channel2", "Channel 2")] DiscordChannel channel_2)

        {

            // BlackList

            ulong[] blackListedUserIds =
            {
                575342593630797825, // MateHUN
            };

            DiscordGuild guild = context.Guild;
            DiscordMember member = await guild.GetMemberAsync(user.Id);

            DiscordEmbedBuilder embed;

            // Check player is blacklisted?

            foreach (var id in blackListedUserIds)
            {
                if (member.Id == id)
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Title = "Drag Members !",
                        Description = "This user blacklisted ! U cannot do that U fckin dumb !",
                        Color = DiscordColor.HotPink
                    };
                    await context.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed: embed));
                    return;
                }
            }

            // Return if No VC

            DiscordVoiceState voiceState = member.VoiceState;
            if (voiceState?.Channel == null)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Drag Members !",
                    Description = "User is not in a voice channel ! Fuckin Bastard !",
                    Color = DiscordColor.HotPink
                };
                await context.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed: embed));
                return;
            }

            // Save offical channel
            DiscordChannel officalChannel = voiceState.Channel;

            embed = new DiscordEmbedBuilder
            {
                Title = "Drag Members !",
                Description = $"Started. [ {user.Mention} ]",
                Color = DiscordColor.HotPink
            };

            await context.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed: embed));

            int dragged = 0;

            try
            {
                while (dragged < 4)
                {
                    await Console.Out.WriteLineAsync("DRAGGED: " + dragged);

                    voiceState = member.VoiceState;


                    while (voiceState?.Channel == null)
                    {
                        Console.WriteLine("User not found! Waiting !");
                        await Task.Delay(550);
                        voiceState = member.VoiceState;
                    }

                    if (voiceState?.Channel != null)
                    {
                        await channel_1.PlaceMemberAsync(member);
                        await Task.Delay(200);
                        await channel_2.PlaceMemberAsync(member);

                        if (dragged == 4)
                        {
                            await Console.Out.WriteLineAsync("Stopped ! LIMIT REACH");
                            break;
                        }
                        dragged++;
                    } else
                    {
                        voiceState = member.VoiceState;
                        await Console.Out.WriteLineAsync($"VoiceState Updated: {voiceState}");

                    }


                }
            } catch (Exception ex) { await Console.Out.WriteLineAsync(ex.ToString()); }

            await officalChannel.PlaceMemberAsync(member);
        }

    }

}
