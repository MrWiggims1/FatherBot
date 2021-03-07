using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Emzi0767.Utilities;
using FatherBotDatabase;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FatherBotDatabase.DataBaseBuilder;

namespace FatherBot
{
    class Program
    {
        static void Main(string[] args)
        {
            RunBot().GetAwaiter().GetResult();
        }

        private static Config Config = new Config().CreateConfig();


        static async Task RunBot()
        {
            if (Config.BotToken == null ||Config.CommandPrefix == null)
                Console.WriteLine("Please fill in config file.");

            var Client = new DiscordClient(new DiscordConfiguration()
            {
                Token = Config.BotToken,
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Debug,
                LogTimestampFormat = "dd MMM yyyy - hh:mm:ss tt"
            });

            var CommandCFG = new CommandsNextConfiguration
            {
                StringPrefixes = new[] { Config.CommandPrefix },
                CaseSensitive = false,
                EnableDms = false
            };

            var Commands = Client.UseCommandsNext(CommandCFG);

            Commands.RegisterCommands<Commands>();

            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;
            Client.MessageCreated += Client_MessageRecieved;

            Client.Ready += Client_ready;

            CheckDatabase(Client);

            await Client.ConnectAsync();

            //await Client.UpdateStatusAsync(activity: new DiscordActivity(Config.BotActivity.Activity, (ActivityType) Enum.Parse(typeof(ActivityType), Config.BotActivity.ActivityType)));

            await Task.Delay(-1);
        }

        private static void CheckDatabase(DiscordClient sender)
        {
            DataBaseBuilder dataBaseBuilder = new DataBaseBuilder(ConfigurationManager.AppSettings["DBFilepath"]);

            if (!dataBaseBuilder.DBExist) {
                sender.Logger.LogWarning($"Database could not be found at `{dataBaseBuilder.FilePath}` creating new database.");

                dataBaseBuilder.CreateNewDataBase();

                sender.Logger.LogInformation($"New database Created.");
            }
            else {
                int columnsAdded = 0;

                sender.Logger.LogInformation("database found, checking for needed updates and applying...");

                var coloumnUpdates = dataBaseBuilder.UpdateSqlTables();

                foreach (var cUpdate in coloumnUpdates) {
                    string columnName = cUpdate.Key;
                    var outcome = cUpdate.Value;

                    switch (outcome.ColumnUpdateEnum) {
                        case ColumnUpdateEnum.ColumnAdded:
                            sender.Logger.LogInformation($"New column added `{columnName}`.");
                            columnsAdded++;
                            break;

                        case ColumnUpdateEnum.ColumnExists:
                            sender.Logger.LogDebug($"Column already exists `{columnName}`.");
                            break;

                        case ColumnUpdateEnum.ColumnError:
                            sender.Logger.LogError($"Error adding `{columnName}`.");
                            throw outcome.Exception;
                    }
                }

                if (columnsAdded == 0) {
                    sender.Logger.LogDebug("Database did not need updating. Deleting backup");
                    File.Delete(dataBaseBuilder.BackupDataBaseFilepath);
                }
                else {
                    sender.Logger.LogInformation($"Database update complete added {columnsAdded} columns. Backup database located at {dataBaseBuilder.BackupDataBaseFilepath}");
                }
            }
        }

        private static Task Client_ready(DiscordClient sender, ReadyEventArgs e)
        {
            sender.Logger.LogInformation($"Discord client is ready, currently in {sender.Guilds.Count} guilds.");

            return Task.CompletedTask;
        }

        private static Task Client_MessageRecieved(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (e.Message.Content.ToLower().StartsWith(Config.CommandPrefix))
                return Task.CompletedTask;

            var responses = DataAccess.Responses.LoadResponseMessages();

            AutoResponder responder = new AutoResponder(sender, e.Message, responses.ToArray());                

            return Task.CompletedTask;
        }

        private static async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            sender.Client.Logger.LogError($"`{e.Context.User.Username}` tried to execute `{e.Command?.QualifiedName ?? "<UnkownCommand>"}` but failed.", e.Exception);

            DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
                .WithReply(e.Context.Message.Id);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder() {
                Title = $"{sender.Client.CurrentUser.Username} encountered an error",
                Color = DiscordColor.Red
            };

            switch (e.Exception) {
                case ChecksFailedException cFe: //Thrown if member does not meet the commands requirements
                    var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
                    StringBuilder CheckBuilder = new StringBuilder();

                    foreach (var check in cFe.FailedChecks) { 
                        switch (check) {
                            case CooldownAttribute cooldown:
                                if (cooldown.MaxUses != 1) 
                                {
                                    CheckBuilder.Append($"{cooldown.GetRemainingCooldown(e.Context):hh\\:mm\\:ss} till next available.");
                                    embed.WithFooter($"You can use this command {cooldown.MaxUses} times within {cooldown.Reset:hh\\:mm\\:ss}");
                                }
                                else
                                    CheckBuilder.Append($"Cool down reset in {cooldown.GetRemainingCooldown(e.Context):hh\\:mm\\:ss}");
                                return;

                            case RequirePrefixesAttribute requirePrefixes:
                                if (requirePrefixes.Prefixes.Length > 1)
                                    CheckBuilder.Append($"\nRequires one of the following prefixes: {String.Join(", ", requirePrefixes.Prefixes)}.");
                                else
                                    CheckBuilder.Append($"\nRequires the prefix: {requirePrefixes.Prefixes.First()}.");
                                return;

                            case RequirePermissionsAttribute perm:
                                CheckBuilder.Append($"\nPermissions required: {perm.Permissions}.");
                                break;

                            case RequireOwnerAttribute ownerOnly:
                                CheckBuilder.Append($"\nThis command can only executed by the bot owner.");
                                break;

                            case RequireNsfwAttribute nsfw:
                                CheckBuilder.Append($"\nThis command can only be executed within a channel marked as NSFW.");
                                break;

                            case RequireGuildAttribute reqGuild:
                                CheckBuilder.Append($"\nThis command can only be executed within a server.");
                                break;

                            case RequireDirectMessageAttribute reqDM:
                                CheckBuilder.Append($"\nThis command can only be executed within {e.Context.Client.CurrentUser.Username}'s DMs.");
                                break;

                            case RequireRolesAttribute reqRole:
                                CheckBuilder.Append($"\nThis command requires you to have {reqRole.CheckMode} of following roles: {String.Join(", ", reqRole.RoleNames)}");
                                break;

                            default:
                                CheckBuilder.Append($"\n{check.GetType().Name}.");
                                break;
                        }
                    }
                    embed.WithTitle("Access denied");
                    embed.WithDescription($"{emoji} You do not have the ability to execute this command. you failed the following checks: {CheckBuilder}");
                    break;

                case CommandNotFoundException cnf:
                    return;

                default:
                    embed.WithDescription(e.Exception.Message ?? "An unexpected error occurred.");
                    break;
            }

            await messageBuilder.WithEmbed(embed.Build()).SendAsync(e.Context.Channel);
        }

        private static Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            sender.Client.Logger.LogInformation($"`{e.Context.User.Username}` executed the `{e.Command.QualifiedName}` command.");

            return Task.CompletedTask;
        }
    }
}
