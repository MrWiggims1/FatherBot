using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FatherBot.Models;
using DSharpPlus.Entities;
using FatherBotDatabase;

namespace FatherBot
{
    class Commands : BaseCommandModule
    {
        internal static async Task ThumbsUpmessage(CommandContext ctx)
        {
            var emoji = DiscordEmoji.FromName(ctx.Client, ":+1:");

            await ctx.Message.CreateReactionAsync(emoji);
        }

        [Command("Profile")]
        async Task GetProfile(CommandContext ctx) => await GetProfile(ctx, ctx.User);

        [Command("Profile")]
        async Task GetProfile(CommandContext ctx, DiscordUser user)
        {
            var profile = DataAccess.Profiles.GetOrCreateProfile(user.Id);

            await ctx.RespondAsync($"Profile Id: {profile.Id}\nGots: {profile.Gots} - {profile.GotRatio}%");
        }

        [Group("Responses")]
        [Description("Manage bot responses.")]
        [RequirePermissions(DSharpPlus.Permissions.Administrator)]
        public class ResponseCommands : BaseCommandModule
        {
            [GroupCommand]
            async Task ShowResponses(CommandContext ctx)
            {
                var responses = DataAccess.Responses.LoadResponseMessages();

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                {
                    Title = "Responses:"
                };

                foreach(var response in responses) {
                    embed.AddField($"Trigger: {response.Trigger}", $"Response: `{response.Response}`\nEnabled: {response.Enabled}\nGots enabled: {response.GiveGot}");
                }

                await ctx.RespondAsync(embed.Build());
            }

            [Command("add")]
            [Description("Add a new response to, must have a unique trigger.")]
            async Task AddResponse(CommandContext ctx, [RemainingText, Description("Format as \"TRIGGER...|RESPONSE...|ENABLED|GIVE GOT\" where ENABLED and GIVE GOT are true or false")] string response)
            {
                var substrings = response.Split('|');
                DataAccess.Responses.AddResponse(substrings[0], substrings[1], Boolean.Parse(substrings[2]), Boolean.Parse(substrings[3]));

                await ThumbsUpmessage(ctx);
            }

            [Command("Remove")]
            [Aliases("rm")]
            [Description("Remove an automatic response.")]
            async Task RemoveResponse(CommandContext ctx, string trigger)
            {
                DataAccess.Responses.RemoveResponse(trigger);
                await ThumbsUpmessage(ctx);
            }

            [Command("Modify")]
            [Aliases("mod")]
            [Description("Modify an existing automatic response.")]
            async Task ModifyREsponse(CommandContext ctx, [RemainingText, Description("Format as \"TRIGGER|RESPONSE|ENABLED|GIVE GOT\" where ENABLED and GIVE GOT are true or false")] string response)
            {
                var substrings = response.Split('|');
                DataAccess.Responses.ModifyResponse(substrings[0], substrings[1], Boolean.Parse(substrings[2]), Boolean.Parse(substrings[3]));
                await ThumbsUpmessage(ctx);
            }
        }
    }
}
