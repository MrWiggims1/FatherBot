using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Emzi0767.Utilities;
using FatherBotDatabase;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
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
            var responses = DataAccess.Responses.LoadResponseMessages();

            AutoResponder responder = new AutoResponder(sender, e.Message, responses.ToArray());                

            return Task.CompletedTask;
        }

        private static Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            sender.Client.Logger.LogError($"`{e.Context.User.Username}` tried to execute `{e.Command.QualifiedName}` but failed.",e.Exception);

            return Task.CompletedTask;
        }

        private static Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            sender.Client.Logger.LogInformation($"`{e.Context.User.Username}` executed the `{e.Command.QualifiedName}` command.");

            return Task.CompletedTask;
        }
    }
}
