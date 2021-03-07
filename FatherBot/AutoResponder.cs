using DSharpPlus;
using DSharpPlus.Entities;
using FatherBot.Models;
using FatherBotDatabase;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FatherBot
{
    class AutoResponder
    {
        private List<ResponseMessage> ResponseMessages { get; set; }

        private Regex Regex { get; set; }

        private Regex ImRegex = new Regex(@"\b(im|i'm|i am)\b", RegexOptions.IgnoreCase);

        private Dictionary<string, ResponseMessage> ResponseDictionary { get; set; }

        public MatchCollection Matches;

        public ResponseMessage Response;

        public bool Responded = false;

        public AutoResponder(DiscordClient client,DiscordMessage message, params ResponseMessage[] responseMessages)
        {
            if (message.Author.IsBot)
                return;

            var profile = DataAccess.Profiles.GetOrCreateProfile(message.Author.Id);
            profile.MessagesSent ++;

            if (profile.IsIgnored)
                return;

            ResponseDictionary = responseMessages.ToDictionary(x => x.Trigger);

            Regex = new Regex(@"\b(" + string.Join('|', responseMessages.Select(x => x.Trigger)) + @")\b", options: RegexOptions.IgnoreCase);

            Matches = ImRegex.Matches(message.Content);

            if (Matches.Count > 0) 
            {
                string subString = message.Content.Substring(Matches.First().Index + Matches.First().Value.Length);

                if (subString.Length > 0) 
                {
                    var responseMsg = new DiscordMessageBuilder()
                            .WithReply(message.Id)
                            .WithContent($"Hi{subString}, Im Dad.")
                            .SendAsync(message.Channel);

                    client.Logger.LogInformation($"{message.Author.Username} sent a message conatining `{Matches.First().Value}`");

                    profile.Gots++;
                    Responded = true;
                }
            }
            else 
            {
                Matches = Regex.Matches(message.Content);

                if (Matches.Count > 0) {
                    Response = ResponseDictionary[Matches.First().Value];

                    var responseMsg = new DiscordMessageBuilder()
                        .WithReply(message.Id)
                        .WithContent(Response.Response)
                        .SendAsync(message.Channel);

                    client.Logger.LogInformation($"{message.Author.Username} sent a message. Responding to '{Response.Trigger}'\n All matches found:", string.Join(',', Matches.Select(x => x.Value)));

                    profile.Gots++;
                    Responded = true;
                }
            }
            

            DataAccess.Profiles.UpadteProfile(profile);
        }
    }
}
