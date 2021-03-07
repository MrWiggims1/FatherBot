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
        [Hidden]
        [Command("TestDb")]
        async Task SendAllResponses(CommandContext ctx)
        {
            var responses = DataAccess.Responses.LoadResponseMessages();

            foreach(ResponseMessage res in responses) 
            {
                Console.WriteLine(res.Id);
            }
        }

        [Command("Profile")]
        async Task GetProfile(CommandContext ctx) => await GetProfile(ctx, ctx.User);

        [Command("Profile")]
        async Task GetProfile(CommandContext ctx, DiscordUser user)
        {
            var profile = DataAccess.Profiles.GetOrCreateProfile(user.Id);

            await ctx.RespondAsync($"Profile Id: {profile.Id}\nGots: {profile.Gots}\nMessages Sent: {profile.MessagesSent}");
        }

        [Group("Responses")]
        [Description("Manage bot responses.")]
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
            async Task AddResponse(CommandContext ctx, [RemainingText, Description("Format as \"TRIGGER|RESPONSE|ENABLED|GIVE GOT\" where ENABLED and GIVE GOT are true or false")] string response)
            {
                var substrings = response.Split('|');
                DataAccess.Responses.AddResponse(substrings[0], substrings[1]);
            }
        }
    }
}
