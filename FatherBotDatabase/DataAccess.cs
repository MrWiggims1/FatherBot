using Dapper;
using FatherBot.Models;
using FatherBotDatabase.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace FatherBotDatabase
{
    public class DataAccess
    {
        public class Responses
        {
            public static List<ResponseMessage> LoadResponseMessages()
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    var output = cnn.Query<ResponseMessage>("SELECT * FROM Responses;", new DynamicParameters());

                    return output.ToList();
                }
            }

            public static void SaveResponseMessage(ResponseMessage responseMessage)
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("INSERT INTO Responses (Id, Trigger, Response, Enabled, GiveGot) values (@Trigger @Response @Enabled @GiveGot);", responseMessage);
                }
            }

            public static ResponseMessage AddResponse(string trigger, string response, bool enabled = true, bool giveGot = true)
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    ResponseMessage newResponse = new ResponseMessage(trigger, response);

                    cnn.Execute($"INSERT INTO Responses (Trigger, Response, Enabled, GiveGot) values (@Trigger, @Response, @Enabled, @GiveGot);", newResponse);

                    var output = cnn.Query($"SELECT * from Responses WHERE Trigger = {trigger}");

                    return output.Single();
                }
            }
        }

        public class Profiles
        {
            public static List<Profile> GetAllProfiles()
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    var output = cnn.Query<Profile>("SELECT * FROM Profiles;", new DynamicParameters());

                    return output.ToList();
                }
            }

            public static Profile GetOrCreateProfile(ulong discordId)
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    var output = cnn.Query<Profile>($"SELECT * FROM Profiles WHERE DiscordId = {discordId};", new DynamicParameters());

                    if (output.Count() == 0)
                    {
                        cnn.Execute($"INSERT INTO Profiles (DiscordId) values ({discordId});");

                        var updatedProfile = GetOrCreateProfile(discordId);

                        return updatedProfile;
                    }

                    else
                        return output.Single();
                }
            }

            public static Profile UpadteProfile(Profile profile)
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("UPDATE Profiles SET MessagesSent = @MessagesSent, DiscordId = @DiscordId, Gots = @Gots, IsIgnored = @IsIgnored WHERE Id = @Id;", profile);

                    var updatedProfile = GetOrCreateProfile(profile.DiscordId);

                    return updatedProfile;
                }
            }

            public static Profile SaveProfile(Profile profile)
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
                {
                    cnn.Execute("INSERT INTO Profiles (DiscordId, Gots, IsIgnored) values (@DiscordId, @Gots, @IsIgnored);", profile);

                    var updatedProfile = GetOrCreateProfile(profile.DiscordId);

                    return updatedProfile;
                }
            }
        }
        protected internal static string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
    }
}
